/* ============================================
 * الملف: Models/OrderItem.cs
 * موديل عناصر الطلب - يمثل منتج واحد داخل طلب
 * يحفظ: اسم المنتج، الكمية، السعر، الخيارات المختارة، ملف التصميم
 * السعر يحفظ كـ Snapshot (لقيمة وقت الطلب وليس الحالي)
 * يمكن أن يكون ProductId = null (إذا حُذف المنتج لاحقاً)
 * ============================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        // يمكن أن يكون Null إذا حُذف المنتج من النظام لاحقاً
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [StringLength(250)]
        public string ProductName { get; set; }

        // الخيارات المختارة كـ JSON (لتسهيل القراءة والعرض)
        // مثال: {"design":"رفع تصميمي","paperType":"كوشيه 350","sides":"وجهين"}
        [Required]
        public string SelectedOptionsJson { get; set; }

        // الإضافات المختارة كـ JSON
        // مثال: [{"id":1,"name":"تصميم خاص","price":250}]
        public string? SelectedExtrasJson { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CalculatedUnitPrice { get; set; }

        // سعر الإضافات الثابت لهذا العنصر
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExtrasTotal { get; set; } = 0m;

        // السعر الإجمالي = (سعر الوحدة × الكمية) + سعر الإضافات
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ItemTotal { get; set; }

        // ملف التصميم المرفوع من العميل (اختياري)
        public int? DesignFileId { get; set; }

        // ملاحظات إضافية من الأدمن (مخفية عن العميل)
        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        // === Navigation Properties ===

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("DesignFileId")]
        public virtual UploadedFile? DesignFile { get; set; }
    }
}