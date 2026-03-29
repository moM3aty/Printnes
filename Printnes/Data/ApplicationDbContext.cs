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
        public DbSet<ProductOption> ProductOptions { get; set; }
        public DbSet<PricingMatrix> PricingMatrices { get; set; }
        public DbSet<QuantityTier> QuantityTiers { get; set; }
        public DbSet<ProductExtra> ProductExtras { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();

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
        }
    }
}