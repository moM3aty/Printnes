using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        public byte? OldStatus { get; set; } // الحالة القديمة (Null إذا كانت هذه أول حركة للطلب)

        [Required]
        public byte NewStatus { get; set; } // الحالة الجديدة التي تم التغيير إليها

        [Required]
        [StringLength(200)]
        public string ChangedBy { get; set; } // من قام بالتغيير (مثال: System أو Admin: Ahmed)

        public string Note { get; set; } // ملاحظات إضافية عن سبب التغيير

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}