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

        public int? ProductId { get; set; } // يمكن أن يكون Null إذا تم حذف المنتج من النظام لاحقاً

        [Required]
        [StringLength(250)]
        public string ProductName { get; set; } // Snapshot لاسم المنتج وقت الطلب (لحفظ التاريخ)

        [Required]
        public string SelectedOptionsJson { get; set; } // JSON يحتوي على الخيارات (ورق، مقاس، إلخ)

        public string SelectedExtrasJson { get; set; } // JSON يحتوي على الإضافات المختارة

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CalculatedUnitPrice { get; set; } // سعر الوحدة وقت الطلب (Snapshot)

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExtrasTotal { get; set; } = 0m; // إجمالي سعر الإضافات لهذا العنصر

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ItemTotal { get; set; } // (الوحدة * الكمية) + الإضافات

        public int? DesignFileId { get; set; } // ملف التصميم المرفوع من العميل

        // Navigation Properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("DesignFileId")]
        public virtual UploadedFile DesignFile { get; set; }
    }
}