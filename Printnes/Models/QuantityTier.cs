/* ============================================
 * الملف: Models/QuantityTier.cs
 * موديل شرائح خصم الكميات
 * يحدد نسبة خصم لكل شريحة كمية
 * مثال: 500-999 نسخة → خصم 10% | 1000-4999 نسخة → خصم 15%
 * يرتبط بمنتج واحد ويمكن أن يكون له شريحة واحدة فقط
 * ============================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class QuantityTier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الحد الأدنى للكمية مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "القيمة يجب أن تكون 1 على الأقل")]
        public int MinQuantity { get; set; }

        [Required(ErrorMessage = "الحد الأقصى للكمية مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "القيمة يجب أن تكون 1 على الأقل")]
        public int MaxQuantity { get; set; }

        [Required(ErrorMessage = "نسبة الخصم مطلوبة")]
        [Range(0.01, 99.99, ErrorMessage = "الخصم يجب أن تكون بين 0.01 و 99.99")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; } = 0m;

        public int SortOrder { get; set; } = 0;

        // Navigation Property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}