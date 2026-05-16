namespace EcommerceApi.DTOs
{
    /// <summary>
    /// DTO for adding item to cart
    /// </summary>
    public class AddToCartRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// DTO for updating cart item quantity
    /// </summary>  
    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }

    /// <summary>
    /// DTO for cart response
    /// </summary>
    public class CartDto
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Total { get; set; }  
        public int ItemCount { get; set; }  
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for individual cart item
    /// </summary>  
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtAddTime { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
