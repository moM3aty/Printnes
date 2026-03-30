// الملف: ViewModels/CartViewModels.cs

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Printnes.ViewModels
{
    // هذا الكلاس وظيفته استقبال وفك تشفير الـ JSON الخاص بالسلة 
    // القادم من الـ LocalStorage في الـ Front-end (مهم لعملية إتمام الطلب)
    public class CartItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("img")]
        public string Img { get; set; }

        [JsonPropertyName("details")]
        public CartItemDetailsDto Details { get; set; }
    }

    public class CartItemDetailsDto
    {
        [JsonPropertyName("design")]
        public string Design { get; set; }

        [JsonPropertyName("sides")]
        public string Sides { get; set; }

        [JsonPropertyName("cover")]
        public string Cover { get; set; }

        [JsonPropertyName("paperType")]
        public string PaperType { get; set; }

        [JsonPropertyName("corners")]
        public string Corners { get; set; }
    }

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