using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("API/[CONTROLLER]")]
    [Produces("application/json")]
    public class CartsController: ControllerBase
    {
        private readonly ECommerceDbContext _context;
        private readonly ILogger<CartsController> _logger;

        public CartsController(ECommerceDbContext context,  ILogger<CartsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get cart for a customer
        /// </summary>
        [HttpGet("{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> GetCart(string customerId)
        {
            _logger.LogInformation("Fetching cart for customer : {CustomerId}", customerId);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if(cart == null)
            {
                _logger.LogWarning("Cart not found for customer : {CustomerId}", customerId);
                return NotFound(new { message = "Cart not found" });
            }

            var dto = MapCartToDto(cart);
            return Ok(dto);
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.CustomerId))
                return BadRequest(new { message = "Customer ID is required" });

            if (request.ProductId <= 0)
                return BadRequest(new { message = "Invalid product ID" });

            if (request.Quantity <= 0)
                return BadRequest(new { message="Quantity must be greater than 0" });

            // Check if product exists and has stock
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product not found : {ProductId}", request.ProductId);
                return NotFound(new { message = "Product not foud" });
            }

            if (product.StockQuantity < request.Quantity)
                return BadRequest(new { message = $"Insufficient Stick. Available : {product.StockQuantity}" });

            //Get or Create cart
            var cart=await _context.Carts
                .Include(c=>c.Items)
                .ThenInclude(ci=>ci.Product)
                .FirstOrDefaultAsync(c=>c.CustomerId == request.CustomerId);

            if(cart == null)
            {
                cart=new Cart { CustomerId = request.CustomerId , CreatedAt=DateTime.UtcNow};
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check if product already in cart
            var existingItem=cart.Items.FirstOrDefault(ci=>ci.ProductId == request.ProductId);

            if(existingItem!=null)
            {
                //Update quantity
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                // Add new Item
                var cartITem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    PriceAtAddTime = product.Price,
                    AddedAt = DateTime.UtcNow,
                };
                _context.CartItems.Add(cartITem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Item added to cart for customer: {CustomerId}, Product : {ProductId}, Quantity : {Quantity}", request.CustomerId, request.ProductId, request.Quantity);

            // Reload cart with items
            cart = await _context.Carts
                .Include(C=>C.Items)
                .ThenInclude(ci=>ci.Product)
                .FirstOrDefaultAsync(c=>c.Id ==cart.Id);

            var dto =MapCartToDto(cart);
            return Ok(dto);
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("items/{cartItemId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            if (request.Quantity <= 0)
                return BadRequest(new { message = "Quantity must be greater than 0" });

            var cartItem = await _context.CartItems
                .Include(ci=> ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if(cartItem== null)
                return NotFound(new {message = "Cart Item not found"});

            // Check stock
            if(cartItem.Product.StockQuantity< request.Quantity)
                return BadRequest(new {message = $"Insufficient stock. Available: {cartItem.Product.StockQuantity}" });

            cartItem.Quantity=request.Quantity;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cart item {CartItemId} updated to quantity : {Quantity}", cartItemId, request.Quantity);

            return Ok(new { message = "Item removed from cart" });
        }

        /// <summary>
        /// Remove Item from cart
        /// </summary>
        [HttpDelete("items/{cartItemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if(cartItem == null)
            {
                _logger.LogWarning("Cart item not found: {CarItemId}", cartItemId);
                return NotFound(new { message = "Cart item not found" });
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cart item {CartItemId} removed", cartItemId);

            return Ok(new { message = "Item removed from cart" });
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        [HttpDelete("{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType (StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearCart(string customerId)
        {
            var cart=await _context.Carts
                .Include(c=>c.Items)
                .FirstOrDefaultAsync(c=>c.CustomerId==customerId);

            if (cart == null)
                return NotFound(new { message = "Cart not found" });

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Cart cleared for customer: {CustomerId}", customerId);

            return Ok(new { message = "Cart cleared successfully" });
        }

        // Helper method
        private CartDto MapCartToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                CustomerId = cart.CustomerId,
                Items = cart.Items.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    PriceAtAddTime = ci.PriceAtAddTime,
                    LineTotal = ci.GetLineTotal(),
                    AddedAt = ci.AddedAt
                }).ToList(),
                Total = cart.GetTotal(),
                ItemCount = cart.GetItemCount(),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };
        }
    }
}
