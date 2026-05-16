namespace EcommerceApi.Models
{
    /// <summary>
    ///  Represents a line item in an order
    ///  Stores product details at time of order (in case product details change later)
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to Order
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Foreign key to Product
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product name snapshot (denormalized for historical record)
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Product price snapshot at time of order
        /// </summary>
        public decimal PriceAtOrderTime { get; set; }

        /// <summary>
        /// Quantity Ordered
        /// </summary>
        public int Quantity { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;

        /// <summary>
        /// Calculate line item total
        /// </summary>
        public decimal GetLineTotal()
        {
            return Quantity * PriceAtOrderTime;
        }

    }
}
