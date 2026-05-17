using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;

namespace EcommerceApi.Data
{
    /// <summary>
    /// Database context for E-Commerce API
    /// Manages all entities and their relationships
    /// </summary>
    public class ECommerceDbContext : DbContext
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
        {
        }

        // DbSets for each entity
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //================PRODUCT====================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Description)
                    .HasMaxLength(1000);

                entity.Property(p => p.Price)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(p => p.StockQuantity)
                    .IsRequired();

                entity.Property(p => p.Category)
                    .HasMaxLength(100);

                entity.Property(p => p.IsActive)
                .HasDefaultValue(true);

                entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

                // Indexes for performance
                entity.HasIndex(p => p.Name).HasDatabaseName("IX_Products_Name");
                entity.HasIndex(p => p.Category).HasDatabaseName("IX_Products_Category");
                entity.HasIndex(p => p.IsActive).HasDatabaseName("IX_Products_IsActive");
            });

            //================CART====================
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Carts");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.CustomerId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // One-to-many relationship with CartItems
                entity.HasMany(c => c.Items)
                    .WithOne(ci => ci.Cart)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================CARTITEM====================
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");
                entity.HasKey(ci => ci.Id);

                entity.Property(ci => ci.Quantity)
                    .IsRequired();

                entity.Property(e => e.PriceAtAddTime)
                .HasPrecision(18, 2)
                .IsRequired();

                entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()");

                // One item can't be in cart twice
                entity.HasIndex(e => new { e.CartId, e.ProductId })
                .IsUnique()
                .HasDatabaseName("UX_CartItems_CartId_ProductId");

                // Relationships
                entity.HasOne(ci => ci.Product)
                   .WithMany(p => p.CartItems)
                   .HasForeignKey(ci => ci.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================ORDER=====================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(ci => ci.Id);

                entity.Property(e => e.CustomerId)
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(e => e.Status)
                .IsRequired();

                entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

                entity.Property(e => e.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

                entity.Property(e => e.CustomerEmail)
                .IsRequired()
                .HasMaxLength(200);

                entity.Property(e => e.Notes)
                .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

                // Indexes for performance
                entity.HasIndex(e => e.CustomerId).HasDatabaseName("IDX_Orders_CustomerId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IDX_Orders_Status");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IDX_Orders_CreatedAt");

                // Relationships
                entity.HasMany(e => e.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Payments)
                .WithOne(oi => oi.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            // ===========================ORDER ITEM============================
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(200);

                entity.Property(e => e.PriceAtOrderTime)
                .HasPrecision(18, 2)
                .IsRequired();

                entity.Property(e => e.Quantity)
                .IsRequired();

                // Relationships
                entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            });


            // ===========================PAYMENT============================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

                entity.Property(e => e.Status)
                .IsRequired();

                entity.Property(e => e.TransactionId)
                .HasMaxLength(200);

                entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");


                // Index for performance : Relationships
                entity.HasIndex(e => e.Status).HasDatabaseName("IDX_Payments_Payments_Status");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IDX_Payments_CreatedAt");
            });
        }
    }
}
