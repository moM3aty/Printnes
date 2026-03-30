/* ============================================
 * الملف: Models/Product.cs
 * موديل المنتج الأساسي - يحتوي كل بيانات المنتج
 * يخزن: اسم، وصف، صورة، سعر، كميات، خيارات، إضافات
 * علاقات: Category, ProductOption, PricingMatrix, QuantityTier, ProductExtra, ProductReview, UserFavorite
 * ============================================ */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [StringLength(250)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [StringLength(250)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(270)]
        public string Slug { get; set; }

        public string? DescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }

        [Required(ErrorMessage = "رابط الصورة مطلوب")]
        [StringLength(500)]
        public string CoverImageUrl { get; set; }

        // نطاق الكميات
        public int MinQuantity { get; set; } = 100;
        public int MaxQuantity { get; set; } = 100000;

        public bool AllowCustomSize { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // === Navigation Properties ===

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        // خيارات المنتج (ورق، مقاس، أوجه، تشطيب)
        [InverseProperty("Product")]
        public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();

        // مصفوفة الأسعار
        [InverseProperty("Product")]
        public virtual ICollection<PricingMatrix> PricingMatrices { get; set; } = new List<PricingMatrix>();

        // شرائح خصم الكميات
        [InverseProperty("Product")]
        public virtual ICollection<QuantityTier> QuantityTiers { get; set; } = new List<QuantityTier>();

        // إضافات اختيارية (تصميم، لفحظ، قص أو حماية)
        [InverseProperty("Product")]
        public virtual ICollection<ProductExtra> ProductExtras { get; set; } = new List<ProductExtra>();

        // عناصر الطلب المرتبطة بالمنتجج
        [InverseProperty("Product")]
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // تقييمات العملاء
        [InverseProperty("Product")]
        public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        // المفضلات
        [InverseProperty("Product")]
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
    }
}