/* ============================================
 * الملف: Models/Banner.cs
 * موديل البانرات (صور متحركة في الـ Slider)
 * يخزن: صورة، عنوان، رابط، ترتيب، حالة النشاط
 * ============================================ */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان البانر مطلوب")]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(500)]
        public string LinkUrl { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}