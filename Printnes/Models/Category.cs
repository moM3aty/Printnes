/* ============================================
 * الملف: Models/Category.cs
 * موديل الأقسم - تصنيف أفقي هرمي
 * يخزن: اسم عربي/إنجليزي، Slug، أيقونة، صورة، قسم أب
 * علاقة: Product (واحد إلى الكثير)
 * ============================================ */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        // ربط القسم بالقائمة الرئيسية في الـ Layout
        // القيم: Stickers, PaperPrints, Boxes, GiftCards, Packaging
        [StringLength(50)]
        public string? ParentMenu { get; set; }

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(220)]
        public string Slug { get; set; }

        [StringLength(100)]
        public string? IconClass { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property - علاقة واحد قسم بمنتجات كثير
        [InverseProperty("Category")]
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}