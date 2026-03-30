/* ============================================
 * الملف: Models/ProductExtra.cs
 * موديل الإضافات الاختيارية للمنتجات
 * إضافات مثل: طلب تصميم خاص، لفحظ، قص، حماية، تغليف فاخر
 * السعر ثابت لا يعتم ضرب في الكمية (يُضاف مرة واحدة)
 * يرتبط بمنتج واحد ويتم إدارته من لوحة التحكم
 * ============================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class ProductExtra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "السعر الثابت مطلوب")]
        [Range(0.01, 99999.99, ErrorMessage = "السعر يجب أن يكون قيمة صحيحة")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        // وصف مختصر للإرشاد العميل
        [StringLength(500)]
        public string? Description { get; set; }

        // نوع الإضافة (للترتيب والفلترة لاحقاً)
        // مثال: Design, Lamination, Foil, UV, DieCut, Fold, etc.
        [StringLength(100)]
        public string? ExtraType { get; set; }

        // هل هذه الإضافة متاحة دائماً للعميل (للتشغيل الديناميكي من الـ API)
        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        // Navigation Property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}