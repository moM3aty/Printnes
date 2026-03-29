using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class QuantityTier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int MinQuantity { get; set; } // مثال: 500

        [Required]
        public int MaxQuantity { get; set; } // مثال: 999

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; } = 0m; // مثال: 10.00 تعني خصم 10%

        // Navigation Property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}