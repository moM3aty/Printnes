/*
 * الملف: Data/ApplicationDbContext.cs
 * تحديث DbContext ليشمل الجداول الجديدة (المفضلات والتقييمات)
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Printnes.Models;

namespace Printnes.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSets (Tables) ---
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CustomDesign> CustomDesigns { get; set; }
        public DbSet<ProductOption> ProductOptions { get; set; }
        public DbSet<PricingMatrix> PricingMatrices { get; set; }
        public DbSet<QuantityTier> QuantityTiers { get; set; }
        public DbSet<ProductExtra> ProductExtras { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Banner> Banners { get; set; }

        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // فهارس فريدة
            modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();

            // علاقة المنتج بالأقسام
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة عناصر الطلب
            modelBuilder.Entity<OrderItem>()
                .HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>()
                .HasOne(o => o.DesignFile)
                .WithMany()
                .HasForeignKey(o => o.DesignFileId)
                .OnDelete(DeleteBehavior.SetNull);

            // علاقات مصفوفة الأسعار (منع الحذف المتتالي)
            modelBuilder.Entity<PricingMatrix>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.PricingMatrices)
                .HasForeignKey(pm => pm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PricingMatrix>()
                .HasOne(pm => pm.PaperOption)
                .WithMany()
                .HasForeignKey(pm => pm.PaperOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PricingMatrix>()
                .HasOne(pm => pm.SizeOption)
                .WithMany()
                .HasForeignKey(pm => pm.SizeOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PricingMatrix>()
                .HasOne(pm => pm.SidesOption)
                .WithMany()
                .HasForeignKey(pm => pm.SidesOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- علاقات التقييمات ---
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Product)
                .WithMany() // يمكن إضافة ICollection<ProductReview> في Product لاحقاً إن أردت
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // فهارس للتقييمات
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => r.ProductId);
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => r.IsApproved);
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => r.Rating);

            // --- علاقات المفضلات ---
            modelBuilder.Entity<UserFavorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // منع التكرار: مستخدم لا يمكنه إضافة نفس المنتج أكثر من مرة
            modelBuilder.Entity<UserFavorite>()
                .HasIndex(f => new { f.UserId, f.ProductId })
                .IsUnique();

            // علاقة سجل الحالات
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.Order)
                .WithMany(o => o.OrderStatusHistories)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة المعاملات المالية
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(t => t.Order)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            // 1. إصلاح علاقة التقييمات
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. إصلاح علاقة المفضلات
            modelBuilder.Entity<UserFavorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. إصلاح علاقة عناصر الطلب
            modelBuilder.Entity<OrderItem>()
                .HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}