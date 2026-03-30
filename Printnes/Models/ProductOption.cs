/* ============================================
 * الملف: Models/ProductOption.cs
 * موديل خيارات المنتج (الورق، المقاس، الأوجه، التشطيب)
 * OptionType: 1=ورق, 2=مقاس, 3=أوجه, 4=تشطيب
 * يرتبط بمنتج واحد ويستخدم في مصفوفة الأسعار
 * ============================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class ProductOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "نوع الخيار مطلوب")]
        [Range(1, 4, ErrorMessage = "نوع الخيار غير صالح")]
        public byte OptionType { get; set; }
        // 1=Paper, 2=Size, 3=Sides, 4=Finish

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        // بيانات إضافية بصيغة JSON (مثل أبعاد المقاس الحقيقية)
        // مثال: {"width": 9, "height": 5.5, "unit": "cm"}
        [StringLength(1000)]
        public string? ExtraData { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}