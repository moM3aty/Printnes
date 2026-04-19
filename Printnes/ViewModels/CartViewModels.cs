using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json; // 👈 مهم جداً عشان الـ JsonElement
using System.Text.Json.Serialization;

namespace Printnes.ViewModels
{
    // هذا الكلاس وظيفته استقبال وفك تشفير الـ JSON الخاص بالسلة 
    // القادم من الـ LocalStorage في الـ Front-end
    public class CartItemDto
    {
        // 🟢 الحل هنا: استخدمنا JsonElement عشان يقبل (رقم) للمنتجات العادية، و (نص) للتصميم المخصص بدون ما يضرب Error
        [JsonPropertyName("id")]
        public JsonElement Id { get; set; }

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

        [JsonPropertyName("SelectedOptionsJson")]
        public string SelectedOptionsJson { get; set; }
    }

    public class CartItemDetailsDto
    {
        [JsonPropertyName("Design")]
        public string Design { get; set; }

        [JsonPropertyName("Sides")]
        public string Sides { get; set; }

        [JsonPropertyName("Cover")]
        public string Cover { get; set; }

        [JsonPropertyName("PaperType")]
        public string PaperType { get; set; }

        [JsonPropertyName("Corners")]
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
        public byte PaymentMethod { get; set; } 

        [Required(ErrorMessage = "السلة فارغة!")]
        public string CartItemsJson { get; set; }
    }
}