using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class ProductExtra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; } // ثابت غير مرتبط بالكمية (مثال: طلب تصميم بـ 250 ريال)

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}