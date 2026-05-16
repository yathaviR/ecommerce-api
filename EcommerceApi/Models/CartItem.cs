namespace EcommerceApi.Models
{
    /// <summary>
    ///  Represents a line item in a shopping cart
    ///  Joins Cart and Product with a quantity
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to Cart
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Foreign key to Product
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Quantity of this product in the cart
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Price snapshot when item was added to cart
        /// (incase product price changes later)
        /// </summary>
        public decimal PriceAtAddTime { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Cart Cart { get; set; }
        public Product Product { get; set; } = null!;

        /// <summary>
        /// Calculate line item total
        /// </summary>
        public decimal GetLineTotal()
        {
            return Quantity * PriceAtAddTime;
        }
    }
}
