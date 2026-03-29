using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } // مثال: PN-2024-001547

        // بيانات العميل
        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [StringLength(250)]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        // العنوان
        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(200)]
        public string District { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(20)]
        public string PostalCode { get; set; }

        // المالية
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal SubTotal { get; set; } // المجموع قبل الضريبة والشحن

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxAmount { get; set; } = 0m; // ضريبة القيمة المضافة

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ShippingCost { get; set; } = 0m;

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; } // المبلغ النهائي

        // الحالات
        [Required]
        public byte PaymentMethod { get; set; } // 1=Mada, 2=Visa, 3=ApplePay, 4=BankTransfer, 5=COD

        [Required]
        public byte PaymentStatus { get; set; } = 0; // 0=Pending, 1=Paid, 2=Failed, 3=Refunded

        [Required]
        public byte OrderStatus { get; set; } = 1; // 1=New, 2=Confirmed, 3=Prepress...

        public string Notes { get; set; } // ملاحظات العميل
        public string InternalNotes { get; set; } // ملاحظات الإدارة (مخفية عن العميل)

        [StringLength(100)]
        public string TrackingNumber { get; set; } // رقم تتبع الشحنة

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // تم إضافة خصائص التنقل (Navigation Properties) لربط الطلب بعناصره وتاريخه وحركات الدفع
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; }
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    }
}