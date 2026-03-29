using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزية مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(220)]
        public string Slug { get; set; } // مثال: business-cards

        [StringLength(100)]
        public string IconClass { get; set; } // مثال: fa-id-card

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; }
    }
}