using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using EcommerceApi.Data;
using EcommerceApi.DTOs;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController:ControllerBase
    {
        private readonly ECommerceDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController (ECommerceDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all orders for a customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrdes(string customerId)
        {
            _logger.LogInformation("Fetching orders for customer: {CustomerId}", customerId);

            var orders= await _context.Orders
                .Where(o=>o.CustomerId==customerId)
                .Include(o=>o.Items)
                .ThenInclude(oi=>oi.Product)
                .Include(o=>o.Payments)
                .OrderByDescending(o=>o.CreatedAt)
                .ToListAsync();

            var dtos = orders.Select(MapOrderToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get a specific order by ID
        /// </summary>
        [HttpGet("orderId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public  async Task<ActionResult<OrderDto>> GetOrder(int orderId)
        {
            _logger.LogInformation("Fetching order: {OrderId}", orderId);

            var order=await _context.Orders
                .Include(o=>o.Items)
                .ThenInclude(oi=>oi.Product)
                .Include(o=>o.Payments)
                .FirstOrDefaultAsync(o=>o.Id== orderId);

            if(order==null)
            {
                _logger.LogWarning("Order not found: {OrderId}", orderId);
                return NotFound(new { message = "Order not found" });
            }

            var dto = MapOrderToDto(order);
            return Ok(dto);
        }

        /// <summary>
        /// Create order from cart
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerId))
                return BadRequest(new { message = "Cutomer ID is required" });

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
                return BadRequest(new { message = "Customer email is required" });

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                return BadRequest(new { message = "Shipping address is required" });

            // Get customer's cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

            if (cart == null || !cart.Items.Any())
                return BadRequest(new { message = "Cart is empty. cannot create order" });

            // Check stock for all items
            foreach (var item in cart.Items)
            {
                if (item.Product.StockQuantity < item.Quantity)
                    return BadRequest(new { message = $"Insufficient stock for product : {item.Product.Name}" });
            }

            // create Order
            var order = new Order
            {
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                ShippingAddress = request.ShippingAddress,
                Notes = request.Notes,
                Status = OrderStatus.Pending,
                TotalAmount = cart.GetTotal(),
                CreatedAt = DateTime.UtcNow
            };

            // Add order items from cart
            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    PriceAtOrderTime = item.PriceAtAddTime,
                    Quantity = item.Quantity
                };
                order.Items.Add(orderItem);

                //Reduce Stock
                item.Product.StockQuantity = item.Quantity;

            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order created with ID: {OrderId} for customer: {CustomerId}", order.Id, request.CustomerId);

            // clear cart
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            var dto = MapOrderToDto(order);
            return CreatedAtAction(nameof(GetOrder), new {OrderId = order.Id}, dto);
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                _logger.LogWarning("Order not found : {OrderId}", orderId);
                return NotFound(new { message = "Order not found" });
            }

            var oldStatus = order.Status;
            order.Status = request.Status;

            order.UpdatedAt = DateTime.UtcNow;

            // Set status-specific timestamps
            if(request.Status == OrderStatus.Paid)
                order.PaidAt = DateTime.UtcNow;
            else if(request.Status != OrderStatus.Shipped)
                order.ShippedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus}", orderId, oldStatus,request.Status);

            return Ok(new {message = "Order status updated successfully"});
        }

        /// <summary>
        /// Cancel an order (if not paid) 
        /// </summary>
        [HttpPost("{orderId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order not found : {OrderId}", orderId);
                return NotFound(new { message = "Order not found" });
            }

            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Shipped)
                return BadRequest(new { message = "Cannot cancel a paid or shipped order" });

            if (order.Status == OrderStatus.Cancelled)
                return BadRequest(new { message = "Order is already cancelled"
                });

            // Restore stock
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} cancelled", orderId);

            return Ok(new { message = "ORder cancelled successfully" });
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrderByStatus(OrderStatus status)
        {
            _logger.LogInformation("Fetching orders with status: {Status}", status);

            var orders = await _context.Orders
                .Where(o=> o.Status == status)
                .Include(o=> o.Items)
                .ThenInclude(oi=>oi.Product)
                .Include(o=>o.Payments)
                .OrderByDescending(o=>o.CreatedAt)
                .ToListAsync();

            var dtos = orders.Select(MapOrderToDto).ToList();
            return Ok(dtos);
        }
        //Helper method
        private OrderDto MapOrderToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CustomerEmail = order.CustomerEmail,
                Notes = order.Notes,
                Items = order.Items.Select(oi => new OrderItemDto
                {
                    Id=oi.Id,
                    ProductId=oi.ProductId,
                    ProductName = oi.ProductName,
                    Quantity    = oi.Quantity,
                    PriceAtOrderTime = oi.PriceAtOrderTime,
                    LineTotal = oi.GetLineTotal()
                }).ToList(),

                Payments = order.Payments.Select(p=> new PaymentDto
                {
                    Id= p.Id,
                    Amount= p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status.ToString(),
                    TransactionId = p.TransactionId,
                    CreatedAt= p.CreatedAt,
                    ProcessedAt= p.ProcessedAt
                }).ToList(),

                ItemCount = order.GetItemCount(),
                IsPaid = order.IsPaid(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                PaidAt = order.PaidAt,
                ShippedAt = order.ShippedAt
            };
        }
    }
}
