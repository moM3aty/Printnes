/* ============================================
 * الملف: Models/Order.cs
 * موديل الطلب الرئيسي - يضم كل بيانات الطلب بالكامل
 * بيانات العميل، المالية، الحالات، العنوان، التاريخ
 * علاقات: OrderItem, OrderStatusHistory, PaymentTransaction, User (اختياري)
 * حالات الطلب: OrderStatus (5 مراحل) و PaymentStatus (4 حالات)
 * ============================================ */

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

        [Required(ErrorMessage = "رقم الطلب مطلوب")]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        // === بيانات العميل ===

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        [StringLength(250)]
        public string? CustomerEmail { get; set; }

        // يمكن ربط الطلب بمستخدم مسجل (اختياري)
        public string? UserId { get; set; }

        // === العنوان ===

        [Required(ErrorMessage = "المدينة مطلوبة")]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(200)]
        public string District { get; set; }

        [Required(ErrorMessage = "العنوان بالتفصيل مطلوب")]
        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        // === المالية ===

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal SubTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxAmount { get; set; } = 0m;

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ShippingCost { get; set; } = 0m;

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; }

        // خصم (يُحسب يدوياً من الإضافات أو الكميات)
        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountAmount { get; set; } = 0m;

        // المبلغ النهائي بعد الخصم والشحن والضريبة
        [NotMapped]
        public decimal FinalAmount
        {
            get
            {
                decimal sub = SubTotal - DiscountAmount;
                decimal tax = sub * (TaxAmount / SubTotal);
                return sub + ShippingCost + tax;
            }
        }

        // === الحالات ===

        [Required]
        public byte PaymentMethod { get; set; }
        // 1=Mada, 2=Visa, 3=ApplePay, 4=تحويل بنكي, 5=COD

        [Required]
        public byte PaymentStatus { get; set; } = 0;
        // 0=Pending, 1=Paid, 2=Failed, 3=Refunded

        [Required]
        public byte OrderStatus { get; set; } = 1;
        // 1=New, 2=Confirmed, 3=Printing, 4=Shipped, 5=Completed

        // === ملاحظات إضافية ===

        [StringLength(2000)]
        public string? Notes { get; set; }

        // ملاحظات داخلية (مخفية عن العميل)
        [StringLength(5000)]
        public string? InternalNotes { get; set; }

        // رقم تتبع الشحنة (مثل Aramex أو SMSA)
        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        // عنوان IP (للحماية الأمان لاحقاً)
        [StringLength(50)]
        public string? IpAddress { get; set; }

        // === التواريخ ===

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // === Navigation Properties ===

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

        // ربط المستخدم (اختياري)
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // === دوال مساعدة لترجمة الحالات ===

        public string GetOrderStatusText()
        {
            return OrderStatus switch
            {
                1 => "جديد",
                2 => "قيد المراجعة",
                3 => "جاري الطباعة",
                4 => "تم الشحن",
                5 => "مكتمل",
                _ => "غير معروف"
            };
        }

        public string GetPaymentStatusText()
        {
            return PaymentStatus switch
            {
                0 => "معلق",
                1 => "مدفوع",
                2 => "فشل",
                3 => "مسترد",
                _ => "غير معروف"
            };
        }

        public string GetPaymentMethodText()
        {
            return PaymentMethod switch
            {
                1 => "بطاقة مدى",
                2 => "بطاقة ائتمان",
                3 => "Apple Pay",
                4 => "تحويل بنكي",
                5 => "الدفع عند الاستلام",
                _ => "غير محدد"
            };
        }
    }
}