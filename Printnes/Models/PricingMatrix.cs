using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class PricingMatrix
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int PaperOptionId { get; set; }

        [Required]
        public int SizeOptionId { get; set; }

        [Required]
        public int SidesOptionId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal BasePrice { get; set; } = 0m; // تكلفة التشغيل (ثابتة بغض النظر عن الكمية)

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; } = 0m; // سعر النسخة الواحدة

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("PaperOptionId")]
        public virtual ProductOption PaperOption { get; set; }

        [ForeignKey("SizeOptionId")]
        public virtual ProductOption SizeOption { get; set; }

        [ForeignKey("SidesOptionId")]
        public virtual ProductOption SidesOption { get; set; }
    }
}