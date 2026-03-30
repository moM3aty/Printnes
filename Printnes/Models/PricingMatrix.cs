/* ============================================
 * الملف: Models/PricingMatrix.cs
 * موديل مصفوفة الأسعار (التقاطعات)
 * يحدد سعر لكل تقاطع: (منتج + ورق + مقاس + أوجه)
 * BasePrice = تكلفة التشغيل الثابتة (مرة واحدة لكل طلب)
 * UnitPrice = سعر النسخة الواحدة (يُضرب في الكمية)
 * ============================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class PricingMatrix
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int PaperOptionId { get; set; }

        [Required]
        public int SizeOptionId { get; set; }

        [Required]
        public int SidesOptionId { get; set; }

        [Required(ErrorMessage = "تكلفة التشغيل مطلوبة")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal BasePrice { get; set; } = 0m;

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; } = 0m;

        // === Navigation Properties (OnDelete.Restrict لمنعع الحذف المتتالي) ===

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("PaperOptionId")]
        public virtual ProductOption PaperOption { get; set; }

        [ForeignKey("SizeOptionId")]
        public virtual ProductOption SizeOption { get; set; }

        [ForeignKey("SidesOptionId")]
        public virtual ProductOption SidesOption { get; set; }
    }
}