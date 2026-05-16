using System.IdentityModel.Tokens.Jwt;

namespace EcommerceApi.Models
{
    /// <summary>
    ///  Represents a customer order (completed purchase)
    /// </summary>
    public class Order
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier for the customer who placed the order
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Order status (Pending, Paid, Shipped, Delivered, Cancelled)
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Total Order Amount
        /// </summary>
        public decimal TotalAmount {  get; set; }

        /// <summary>
        /// Shipping address
        /// </summary>
        public string ShippingAddress { get; set; } = string.Empty;

        /// <summary>
        /// Customer email for order confirmation
        /// </summary>
        public string CustomerEmail {  get; set; } = string.Empty;

        /// <summary>
        /// Special notes or instructions from customer
        /// </summary>
        public string? Notes {  get; set; }

        /// <summary>
        /// When the order was placed
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the order was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// When the order was paid (if applicable)
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// When the orer was shipped
        /// </summary>
        public DateTime? ShippedAt { get; set; }

        // Navigation properties
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        /// <summary>
        /// Get total number of items in order
        /// </summary>
        public int GetItemCount()
        {
            return Items.Sum(item => item.Quantity);
        }

        /// <summary>
        /// Check if order is paid
        /// </summary>
        public bool IsPaid()
        {
            return Status == OrderStatus.Paid || Payments.Any(p=> p.Status == PaymentStatus.Completed);
        }
    }

    /// <summary>
    /// Order status enum
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    } 
}
