namespace EcommerceApi.Models
{
    /// <summary>
    /// Represents a product in the e-commerce store. 
    /// </summary>
    public class Product
    {
        public int Id { get; set; }

        /// <summary>
        /// Product name, required 
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product Description
        /// </summary>  
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product price in currency units, required
        /// </summary>  
        public decimal Price { get; set; }

        /// <summary>
        /// Number of units in stock
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Product category / type
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Whether the product is active and available for purchase
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the product was added to the store
        /// </summary>  
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the product was last updated
        /// </summary>  
        public DateTime? UpdatedAt { get; set; }
        

        /// Navigation properties
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
