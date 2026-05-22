using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using EcommerceApi.Data;
using EcommerceApi.DTOs;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/(controller)")]
    [Produces("application/json")]
    public class PaymentsController: ControllerBase
    {
        private readonly ECommerceDbContext _context;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ECommerceDbContext context, ILogger<PaymentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all payments for an order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetOrderPayments(int orderId)
        {
            _logger.LogInformation("Fetching payments for order: {OrderId}", orderId);

            var payments = await _context.Payments
                .Where(p=>p.OrderId == orderId)
                .OrderByDescending(p=>p.CreatedAt)
                .ToListAsync();

            var dtos = payments.Select(MapPaymentToDto).ToList();
            return Ok(dtos);
         }

        /// <summary>
        /// Get a specific payment
        /// </summary>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPayment(int paymentId)
        {
            _logger.LogInformation("Fetching payment: {paymentId}", paymentId);

            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", paymentId);
                return NotFound(new { message = "Payment not found" });
            }

            var dto=MapPaymentToDto(payment);
            return Ok(dto); 
        }

        /// <summary>
        /// Process a payment for an order
        /// </summary> 
        [HttpPost("process/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(int orderId, [FromBody] ProcessPaymentRequest request)
        {
            // Validate input
            if (request.Amount <= 0)
                return BadRequest(new { message = "Payment amount must be greater than 0" });
            if(string.IsNullOrWhiteSpace(request.PaymentMethod))
                return BadRequest(new { message = "Payment method is required" });

            // Get order
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
               _logger.LogWarning("Order not found: {OrderId}", orderId);
                return NotFound(new { message = "Order not found" });
            }

            // validate amount
            if (request.Amount > order.TotalAmount)
                return BadRequest(new { message = "Payment amount exceeds order total. Order total : {order.TotalAmount}" });

            // Create payment record
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Simulate payment processing (in real app, integrate with payment gateway like Stripe)
            try
            {
                // Here you would call payment gateway AP (Stripe, Paypal, etc.)
                // For now we will simulate success with 80% probability
                var random = new Random();
                bool paymentSucceeds = random.Next(100) < 80;

                if (paymentSucceeds)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.TransactionId = $"TXN-{DateTime.UtcNow.Ticks}"; // Simulated transaction ID
                    payment.ProcessedAt = DateTime.UtcNow;

                    //Update order status if full amount paid
                    if (request.Amount >= order.TotalAmount)
                    {
                        order.Status = OrderStatus.Paid;
                        order.PaidAt = DateTime.UtcNow;
                        order.UpdatedAt = DateTime.UtcNow;
                    }

                    _logger.LogInformation("Payment processed successfully for order: {OrderId}, Amount: {Amount}", orderId, request.Amount);
                }
                else
                {

                    payment.Status = PaymentStatus.Failed;
                    payment.ErrorMessage = "Payment gateway declined the transaction";
                    payment.ProcessedAt = DateTime.UtcNow;

                    _logger.LogWarning("Payment failed for order: {OrderId}, Amount: {Amount}", orderId, request.Amount);
                }
            }
            catch(Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage= $"Payment processing exception for order : {ex.Message}";
                payment.ProcessedAt= DateTime.UtcNow;

                _logger.LogError(ex, "Payment processing exception for order : {OrderId}", orderId);
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var dto=MapPaymentToDto(payment);
            if (payment.Status == PaymentStatus.Completed)
                return Ok(dto);

            else
                return BadRequest(new { message = "Payment failed", payment = dto });
        }

        /// <summary>
        /// Refund a payment
        /// </summary>
        [HttpPost("{paymentId}/refund")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefundPayment(int paymentId, [FromBody] RefundRequest request)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for refund: {PaymentId}", paymentId);
                return NotFound(new { message = "PAyment not found" });
            }

            if (payment.Status != PaymentStatus.Completed)
                return BadRequest(new { message = "Only completed payments can be refunded" });

            if (payment.Status == PaymentStatus.Refunded)
                return BadRequest(new { message = "Payment is already refunded" });

            // Process refund (in real app, call payment gateway refund API)
            payment.Status = PaymentStatus.Refunded;
            payment.ProcessedAt = DateTime.UtcNow;

            //Update order status
            if (payment.Order != null && payment.Order.Status == OrderStatus.Paid)
            {
                payment.Order.Status = OrderStatus.Cancelled;
                payment.Order.UpdatedAt = DateTime.UtcNow;

                //Restore stock
                var orderItems = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.OrderId == payment.OrderId)
                    .ToListAsync();

                foreach (var item in orderItems)
                {
                    item.Product.StockQuantity += item.Quantity;
                }
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} refunded", paymentId);

            return Ok(new { message = "Payment refunded successfully" });
        }

        /// <summary>
        ///  Get payment statistics
        /// </summary>
        [HttpGet("stats/daily")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetDailyPaymentStatus()
        {
            _logger.LogInformation("Fetching daily payment statistics");

            var today = DateTime.UtcNow.Date;

            var stats = await _context.Payments
                .Where(p=> p.CreatedAt.Date==today && p.Status ==PaymentStatus.Completed)
                .GroupBy(p=>p.PaymentMethod)
                .Select(g=> new
                {
                    PaymentMethod = g.Key,
                    TotalAmount = g.Sum(p=> p.Amount),
                    TransactionCount = g.Count(),
                    AverageAmount = g.Average(p=> p.Amount)
                })
                .ToListAsync();
            return Ok(stats);
        }

        // Helper method
        private PaymentDto MapPaymentToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status.ToString(),
                TransactionId = payment.TransactionId,
                ErrorMessage = payment.ErrorMessage,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt
            };
        }

    }
}
