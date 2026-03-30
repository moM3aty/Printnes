/* ============================================
 * الملف: Models/OrderStatusHistory.cs
 * موديل سجل تحديثات حالة الطلب
 * يسجل كل تغيير في حالة الطلب (مع الحالة القديمة والجديدة والملاحظة)
 * يُستخدم لعرض تاريخ التحديثات في صفحة تفاصيل الطلب
 * ============================================ */

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

        // الحالة القديمة (Null إذا كانت أول تحديث)
        public byte? OldStatus { get; set; }

        // الحالة الجديدة
        [Required]
        public byte NewStatus { get; set; }

        // من قام بالتحديث (اسم المستخدم أو "System")
        [Required]
        [StringLength(200)]
        public string ChangedBy { get; set; }

        // ملاحظة إضافية
        [StringLength(2000)]
        public string Note { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}