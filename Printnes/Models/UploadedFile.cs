using System;
using System.ComponentModel.DataAnnotations;

namespace Printnes.Models
{
    public class UploadedFile
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الملف الفعلي مطلوب")]
        [StringLength(500)]
        public string FileName { get; set; } // اسم الملف الفعلي على السيرفر (مثال: GUID.pdf)

        [Required(ErrorMessage = "الاسم الأصلي للملف مطلوب")]
        [StringLength(500)]
        public string OriginalName { get; set; } // اسم الملف الأصلي من جهاز العميل

        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; } // المسار الكامل للملف

        [Required]
        public long FileSizeBytes { get; set; } // حجم الملف بالبايت

        [StringLength(100)]
        public string MimeType { get; set; }

        [StringLength(200)]
        public string SessionId { get; set; } // مرتبط بسلة العميل (Guest) قبل إتمام الطلب

        public bool IsLinked { get; set; } = false; // يتغير لـ true لما يتم ربط الملف بطلب فعلي

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}