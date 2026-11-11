using System;
using System.Threading;
using System.Threading.Tasks;
using Grandis.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Grandis.Data;

public class GrandisDbContext : DbContext
{
    public GrandisDbContext(DbContextOptions<GrandisDbContext> options) : base(options)
    {
    }

    public DbSet<Addressee> Addressees => Set<Addressee>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Addressee>(entity =>
        {
            entity.ToTable("Addressee", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AddresseeCountry).HasMaxLength(2).IsRequired();
            entity.Property(e => e.AddresseeZip).HasMaxLength(12).IsRequired();
            entity.Property(e => e.AddresseeAddress).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AddresseeCity).HasMaxLength(25).IsRequired();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(250).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreditedAt).HasColumnType("datetime").HasDefaultValueSql("getutcdate()").IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(25).IsUnicode(false);

            entity.HasOne(c => c.Addressee)
                  .WithMany()
                  .HasForeignKey(c => c.AddresseeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.AddresseeId, "IX_Customer_AddresseeId");
            entity.HasIndex(c => c.Email, "IX_Customer_Email");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Supplier", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("getutcdate()").IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description);
            entity.Property(e => e.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.StockQuantity).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();

            // Relációk
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Supplier)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexek
            entity.HasIndex(p => p.CategoryId, "IX_Product_CategoryId");
            entity.HasIndex(p => p.SupplierId, "IX_Product_SupplierId");
            entity.HasIndex(p => new { p.IsActive, p.Price }, "IX_Product_IsActive_Price");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Order", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderDate).HasColumnType("datetime").IsRequired();
            entity.Property(e => e.StatusId).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();

            entity.HasOne(o => o.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(o => o.CustomerId, "IX_Order_CustomerId");
            entity.HasIndex(o => new { o.OrderDate, o.StatusId }, "IX_Order_OrderDate_StatusId");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItem", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2).IsRequired();

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(oi => oi.OrderId, "IX_OrderItem_OrderId");
            entity.HasIndex(oi => oi.ProductId, "IX_OrderItem_ProductId");
        });
    }
}