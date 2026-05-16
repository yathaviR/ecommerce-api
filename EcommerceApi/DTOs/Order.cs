using EcommerceApi.Models;

namespace EcommerceApi.DTOs
{
    /// <summary>
    ///  DTO for creating an order from cart
    /// </summary>
    /// 
    public class CreateOrderRequest 
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating order status
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }

    /// <summary>
    /// DTO for order response
    /// </summary>  
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
        public int ItemCount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
    }

    /// <summary>
    /// DTO for order line item
    /// </summary>
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }= string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtOrderTime { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// DTO for paymebt information
    /// </summary>
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

}
