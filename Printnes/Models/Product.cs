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

        [Required(ErrorMessage = "اسم المنتج بالعربية مطلوب")]
        [StringLength(250)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "اسم المنتج بالإنجليزية مطلوب")]
        [StringLength(250)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(270)]
        public string Slug { get; set; }

        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }

        [Required]
        [StringLength(500)]
        public string CoverImageUrl { get; set; }

        public int MinQuantity { get; set; } = 100;
        public int MaxQuantity { get; set; } = 100000;

        public bool AllowCustomSize { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        // تم تفعيل خصائص التنقل (Navigation Properties) لربط العلاقات بشكل صحيح
        public virtual ICollection<ProductOption> ProductOptions { get; set; }
        public virtual ICollection<PricingMatrix> PricingMatrices { get; set; }
        public virtual ICollection<QuantityTier> QuantityTiers { get; set; }
        public virtual ICollection<ProductExtra> ProductExtras { get; set; }
    }
}