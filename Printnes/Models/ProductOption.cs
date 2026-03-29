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

        [Required]
        public byte OptionType { get; set; } // 1=Paper, 2=Size, 3=Sides, 4=Finish

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [StringLength(500)]
        public string ExtraData { get; set; } // بيانات إضافية JSON (مثال: {"width": 9, "height": 5.5})

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}