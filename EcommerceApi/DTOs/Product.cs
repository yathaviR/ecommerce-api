namespace EcommerceApi.DTOs
{
    /// <summary>
    /// DTO for creating a new product. This class is used to transfer data when a client wants to create a new product in the system. It includes properties such as Name, Description, Price, StockQuantity, and Category, which are necessary for defining the attributes of the product being created.
    /// </summary>
    public class ProductCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for updating a product. This class is used to transfer data when a client wants to update an existing product in the system. It includes properties such as Name, Description, Price, StockQuantity, Category, and IsActive, which are necessary for defining the attributes of the product being updated.
    /// </summary>   
    public class ProductUpdateRequest
    {
        public string? Name { get; set; } 
        public string? Description { get; set; } 
        public decimal? Price { get; set; }
        public int? StockQuantity { get; set; }
        public string? Category { get; set; } 
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for product responses
    /// </summary>  
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
