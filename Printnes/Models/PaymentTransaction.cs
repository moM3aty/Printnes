/* ============================================
 * الملف: Models/PaymentTransaction.cs
 * موديل المعاملات المالية - يسجل كل عملية دفع فعلية
 * يحفظ: البوابة المستخدم، الـ ID الراجع، المبلغ، الحالة
 * يُستخدم لاحقاً مع بوابات الدفع (Moyasar, MyFatoorah, Tap)
 * ============================================ */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class PaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        // اسم بوابة الدفع
        [Required]
        [StringLength(50)]
        public string ProviderName { get; set; }
        // مثال: Moyasar, MyFatoorah, Tap, BankTransfer

        // الـ ID الراجع من البوابة (للتتبع في حال المشاكل)
        [StringLength(200)]
        public string? ProviderTransactionId { get; set; }

        // المبلغ المدفوع
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        // حالة المعاملة
        [Required]
        public byte Status { get; set; }
        // 0=Initiated, 1=Success, 2=Failed, 3=Refunded

        // الرد الخام الكامل من البوابة (JSON) - مهم للمراجعة لاحقاً
        [Column(TypeName = "nvarchar(max)")]
        public string? RawResponseJson { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}