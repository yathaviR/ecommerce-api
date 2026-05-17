using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using System.Collections;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController:ControllerBase
    {
        private readonly ECommerceDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ECommerceDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all active products with optional filtering
        /// </summary>
        /// <param name="category">Filter by category (optional) </param>
        /// <param name="minPrice">Minimum price filter (optional) </param>
        /// <param name="maxPrice">Maximum price filter (optional) </param>
        /// <param name="skip">Number of records to skip (pagination) </param>
        /// <param name="take">Number of records to return (pagination)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] string? category=null,
            [FromQuery] decimal? minPrice=null,
            [FromQuery] decimal? maxPrice=null,
            [FromQuery] int skip=0,
            [FromQuery] int take=20)
        {
            try
            {
                _logger.LogInformation("Fetching products with filters - Catgory : {Category}, PriceRange : {Min}-{Max}", category, minPrice, maxPrice);

                var query = _context.Products.Where(p => p.IsActive).AsQueryable();

                // Apply Filters
                if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(p => p.Category.ToLower() == category.ToLower());

                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);

                var products = await query
                    .OrderBy(p => p.Name)
                    .Skip(skip)
                    .Take(take)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        Category = p.Category,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(products);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return StatusCode(500, new { message="Error fetching products", error=ex.Message});
            }
        }

        /// <summary>
        /// Get a specific product by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            _logger.LogInformation("Fetching product with ID: {ProductId}", id);

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId}", id);
                return NotFound(new { message = "Product not found" });
            }

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductCreateRequest request)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Product name is required" });

            if (request.Price <= 0)
                return BadRequest(new { message = "Product price must be greater than 0" });

            if (request.StockQuantity <= 0)
                return BadRequest(new { message = "Stock quantity cannot be negative" });

            var product = new Product
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Category = request.Category?.Trim() ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created with ID: {ProductId}", product.Id);

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                IsActive = product.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, dto);
        }

        /// <summary>
        /// update a product (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Update failed: Product with ID {ProductId} not found", id);
                return NotFound(new { message = "Product not found" });
            }

            // Validate updates
            if (request.Price.HasValue && request.Price <= 0)
                return BadRequest(new { message = "Product price must be greater than 0" });
            if (request.StockQuantity.HasValue && request.StockQuantity < 0)
                return BadRequest(new { message = "Stock quantity cannot be negative" });

            //Update fields
            if (!string.IsNullOrWhiteSpace(request.Name))
                product.Name = request.Name.Trim();

            if(request.Description!=null)
                product.Description = request.Description.Trim();

            if(request.Price.HasValue)
                product.Price = request.Price.Value;

            if(request.StockQuantity.HasValue)
                product.StockQuantity = request.StockQuantity.Value;

            if(!string.IsNullOrWhiteSpace(request.Category))
                product.Category = request.Category.Trim();

            if(request.IsActive.HasValue)
                product.IsActive = request.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} updated", id);

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
            };

            return Ok(new { message = "Product updated successfully", product = dto });
        }

        /// <summary>
        /// Delete a product (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                _logger.LogWarning("Delete failed : Product with ID {ProductId} not found", id);
                return NotFound(new { message = "Product not found" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} deleted", id);

            return Ok(new { message = "Product deleted successfully" });
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(string category)
        {
            _logger.LogInformation("Fetching products for category: {Category}", category);

            var products = await _context.Products
                .Where(p => p.IsActive && p.Category.ToLower() == category.ToLower())
                .OrderBy(p => p.Price)
                .Select(p => new ProductDto
                {
                    Id=p.Id,
                    Name=p.Name,
                    Description=p.Description,
                    Price=p.Price,
                    StockQuantity=p.StockQuantity,
                    Category=p.Category,
                    IsActive=p.IsActive,
                    CreatedAt=p.CreatedAt,
                    UpdatedAt =p.UpdatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// Search products by name
        /// </summary>
        [HttpGet("search/{query}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(string query)
        {
            _logger.LogInformation("Searching products wuth query: {Query}", query);

            var products = await _context.Products
                .Where(p=>p.IsActive && p.Name.ToLower().Contains(query.ToLower()))
                .OrderBy(p => p.Name)
                .Select(p=> new  ProductDto
                {
                    Id=p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price   = p.Price,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt=p.UpdatedAt
                })
                .ToListAsync();

            return Ok(products);
        }
    }
}
