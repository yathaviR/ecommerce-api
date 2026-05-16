namespace EcommerceApi.Models
{
    /// <summary>
    /// Represents a shopping cart belonging to a customer.
    /// </summary>
    public class Cart
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique Identifier for the customer who owns this cart
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// When the cart was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the cart was last modified
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        /// <summary>
        /// Calculate total cart value
        /// </summary>
        public decimal GetTotal()
        {
            return Items.Sum(item=> item.Quantity * item.Product.Price);
        }

        /// <summary>
        /// Get total number of items in cart
        /// </summary>
        public int GetItemCount()
        {
            return Items.Sum(item => item.Quantity);
        }
    }
}
