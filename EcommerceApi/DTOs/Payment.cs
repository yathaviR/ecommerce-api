using EcommerceApi.Models;

namespace EcommerceApi.DTOs
{
    /// <summary>
    /// DTO for processing a payment
    /// </summary>
    public class ProcessPaymentRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CardToken { get; set; } // For Payment Gateway integration
    }

    /// <summary>
    /// DTO for payment response
    /// </summary>
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    /// <summary>
    /// DTO for refund request
    /// </summary>
    public class RefundRequest
    {
        public string Reason { get; set; }= string.Empty;
    }
}
