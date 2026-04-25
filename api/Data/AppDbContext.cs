using KeycloakAuth.Entities;
using KeycloakAuth.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeycloakAuth.Data;

/// <summary>
/// Database context for Adega Royal, using primary constructor for dependency injection.
/// All entity mappings are defined via Fluent API in OnModelCreating.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // === ADEGA ROYAL CORE ENTITIES ===
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =============================================
        // USER CONFIGURATION
        // =============================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();

            entity.Property(u => u.KeycloakId)
                .IsRequired()
                .HasMaxLength(200);
            entity.HasIndex(u => u.KeycloakId)
                .IsUnique()
                .HasDatabaseName("UX_User_KeycloakId");

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(320);
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UX_User_Email");

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);
        });

        // =============================================
        // CATEGORY CONFIGURATION
        // =============================================
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("UX_Category_Name");

            entity.Property(c => c.Description)
                .HasMaxLength(500);
        });

        // =============================================
        // PRODUCT CONFIGURATION
        // =============================================
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(1000);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(p => p.StockQuantity)
                .IsRequired();

            entity.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            entity.Property(p => p.IsActive)
                .HasDefaultValue(true);

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Product_CategoryId");
            entity.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Product_Name");
        });

        // =============================================
        // CART CONFIGURATION
        // =============================================
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();

            entity.Property(c => c.UserId)
                .IsRequired()
                .HasMaxLength(200);

            // Cart is the DEPENDENT side of the 1:1 with User.
            // UserId stores the User.KeycloakId (string FK).
            // We intentionally do NOT use a direct PK-FK to User here
            // because User.Id is a Guid while Cart.UserId is the KeycloakId string.
            // Navigation is informational only — no EF-enforced FK to User table.
            entity.Ignore(c => c.User);

            entity.HasIndex(c => c.UserId)
                .IsUnique()
                .HasDatabaseName("UX_Cart_UserId");

            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // =============================================
        // CART ITEM CONFIGURATION
        // =============================================
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Id).ValueGeneratedNever();

            entity.Property(ci => ci.Quantity)
                .IsRequired();

            entity.Property(ci => ci.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate products in the same cart
            entity.HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique()
                .HasDatabaseName("UX_CartItem_CartId_ProductId");

            entity.HasIndex(ci => ci.CartId)
                .HasDatabaseName("IX_CartItem_CartId");
        });

        // =============================================
        // ORDER CONFIGURATION
        // =============================================
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedNever();

            entity.Property(o => o.UserId)
                .IsRequired()
                .HasMaxLength(200);

            // Order.UserId is the Keycloak ID (string), not a FK to User.Id (Guid).
            // Ignore the navigation to avoid EF creating a shadow property UserId1.
            entity.Ignore(o => o.User);

            entity.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.Notes)
                .HasMaxLength(500);

            entity.HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Order_UserId");
            entity.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_Order_CreatedAt");
            entity.HasIndex(o => o.Status)
                .HasDatabaseName("IX_Order_Status");
        });

        // =============================================
        // ORDER ITEM CONFIGURATION
        // =============================================
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.Id).ValueGeneratedNever();

            entity.Property(oi => oi.Quantity)
                .IsRequired();

            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(oi => oi.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(oi => oi.OrderId)
                .HasDatabaseName("IX_OrderItem_OrderId");
            entity.HasIndex(oi => oi.ProductId)
                .HasDatabaseName("IX_OrderItem_ProductId");
        });

        // =============================================
        // DELIVERY CONFIGURATION
        // =============================================
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedNever();

            entity.Property(d => d.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(d => d.VerificationCode)
                .IsRequired()
                .HasMaxLength(4)
                .IsFixedLength();

            entity.Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // 1:1 relationship — one Delivery per Order
            entity.HasOne(d => d.Order)
                .WithOne(o => o.Delivery)
                .HasForeignKey<Delivery>(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(d => d.OrderId)
                .IsUnique()
                .HasDatabaseName("UX_Delivery_OrderId");
        });
    }
}