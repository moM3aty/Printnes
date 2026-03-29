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

        [Required]
        [StringLength(50)]
        public string ProviderName { get; set; } // بوابة الدفع (مثال: Moyasar, MyFatoorah, Tap)

        [StringLength(200)]
        public string ProviderTransactionId { get; set; } // الـ ID الراجع من بوابة الدفع للرجوع إليه وقت المشاكل

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        [Required]
        public byte Status { get; set; } // 0=Initiated, 1=Success, 2=Failed

        public string RawResponseJson { get; set; } // الرد الخام من البوابة (Response) للحفظ والمراجعة

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}