using AdegaRoyal.Api.Entities;
using AdegaRoyal.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace AdegaRoyal.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPassword> UserPasswords => Set<UserPassword>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
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

        // ── USER ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();

            entity.Property(u => u.Name).IsRequired().HasMaxLength(200);

            entity.Property(u => u.Email).IsRequired().HasMaxLength(320);
            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UX_User_Email");

            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });

        // ── USER PASSWORD (one-to-one) ─────────────────────────────────────────
        modelBuilder.Entity<UserPassword>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();

            entity.Property(p => p.PasswordHash).IsRequired().HasMaxLength(100);

            entity.HasOne(p => p.User)
                .WithOne(u => u.Password)
                .HasForeignKey<UserPassword>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Guarantees no user has two password rows
            entity.HasIndex(p => p.UserId).IsUnique().HasDatabaseName("UX_UserPassword_UserId");
        });

        // ── USER CLAIMS (one-to-many) ──────────────────────────────────────────
        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();

            entity.Property(c => c.ClaimValue).IsRequired().HasMaxLength(200);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── REFRESH TOKENS (one-to-many) ───────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedNever();

            entity.Property(r => r.Token).IsRequired().HasMaxLength(200);
            entity.HasIndex(r => r.Token).IsUnique().HasDatabaseName("UX_RefreshToken_Token");

            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CATEGORY ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique().HasDatabaseName("UX_Category_Name");
            entity.Property(c => c.Description).HasMaxLength(500);
        });

        // ── PRODUCT ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.StockQuantity).IsRequired();
            entity.Property(p => p.ImageUrl).HasMaxLength(500);
            entity.Property(p => p.IsActive).HasDefaultValue(true);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── CART ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();
            entity.Property(c => c.UserId).IsRequired().HasMaxLength(200);
            entity.Ignore(c => c.User);
            entity.HasIndex(c => c.UserId).IsUnique().HasDatabaseName("UX_Cart_UserId");
        });

        // ── CART ITEM ─────────────────────────────────────────────────────────
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Id).ValueGeneratedNever();
            entity.Property(ci => ci.Quantity).IsRequired();

            entity.HasOne(ci => ci.Cart).WithMany(c => c.Items).HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ci => ci.Product).WithMany(p => p.CartItems).HasForeignKey(ci => ci.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ORDER ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedNever();
            entity.Property(o => o.UserId).IsRequired().HasMaxLength(200);
            entity.Ignore(o => o.User);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(o => o.Notes).HasMaxLength(500);
        });

        // ── ORDER ITEM ────────────────────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.Id).ValueGeneratedNever();
            entity.Property(oi => oi.Quantity).IsRequired();
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2).IsRequired();

            entity.HasOne(oi => oi.Order).WithMany(o => o.OrderItems).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(oi => oi.Product).WithMany(p => p.OrderItems).HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── DELIVERY ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedNever();
            entity.Property(d => d.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(d => d.VerificationCode).IsRequired().HasMaxLength(4).IsFixedLength();

            entity.HasOne(d => d.Order).WithOne(o => o.Delivery).HasForeignKey<Delivery>(d => d.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
