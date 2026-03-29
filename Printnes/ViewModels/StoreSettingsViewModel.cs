using System.ComponentModel.DataAnnotations;

namespace Printnes.ViewModels
{
    // هذا الكلاس ضروري جداً لكي تعمل صفحة الإعدادات الموجودة في الـ Canvas
    public class StoreSettingsViewModel
    {
        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [StringLength(100)]
        public string StoreName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "نسبة الضريبة مطلوبة")]
        [Range(0, 100, ErrorMessage = "النسبة يجب أن تكون بين 0 و 100")]
        public decimal TaxPercentage { get; set; }

        [Required(ErrorMessage = "تكلفة الشحن مطلوبة")]
        [Range(0, 1000, ErrorMessage = "قيمة غير منطقية")]
        public decimal DefaultShippingCost { get; set; }
    }
}