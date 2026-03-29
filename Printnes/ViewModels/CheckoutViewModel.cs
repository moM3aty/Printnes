using System.ComponentModel.DataAnnotations;

namespace Printnes.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "الاسم الكريم مطلوب")]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب للتواصل")]
        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "المدينة مطلوبة")]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(200)]
        public string District { get; set; }

        [Required(ErrorMessage = "العنوان بالتفصيل مطلوب لتسهيل التوصيل")]
        [StringLength(500)]
        public string Address { get; set; }

        public string PostalCode { get; set; }

        public string Notes { get; set; }

        [Required]
        public byte PaymentMethod { get; set; } // 1=Mada, 2=Visa, 3=ApplePay, 4=BankTransfer, 5=COD

        // هذا الحقل سيستقبل بيانات السلة من الـ LocalStorage عن طريق الـ JavaScript كـ JSON String
        [Required(ErrorMessage = "السلة فارغة!")]
        public string CartItemsJson { get; set; }
    }
}