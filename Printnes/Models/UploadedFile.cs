/* ============================================
 * الملف: Models/UploadedFile.cs
 * موديل الملفات المرفوعة من العملاء (تصاميمات)
 * يحفظ: اسم الملف الأصلي، الاسم الفعلي، المسار، الحجم، النوع (MIME)
 * يُستخدم لربط ملف التصميم بـ OrderItem
 * يُخزن في مجلد uploads/designs في wwwroot
 * ============================================ */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class UploadedFile
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الملف الفعلي مطلوب")]
        [StringLength(500)]
        public string OriginalName { get; set; }

        [Required(ErrorMessage = "اسم الملف على السيرفر مطلوب")]
        [StringLength(500)]
        public string FileName { get; set; }

        [Required(ErrorMessage = "مسار الملف مطلوب")]
        [StringLength(1000)]
        public string FilePath { get; set; }

        [Required(ErrorMessage = "حجم الملف بالبايت مطلوب")]
        public long FileSizeBytes { get; set; }

        // نوع الملف (image/pdf/ai/psd/etc.)
        [StringLength(100)]
        public string MimeType { get; set; }

        // معرف جلسة الـ Session للربط الملف بالسلة قبل الإتمام
        [StringLength(200)]
        public string? SessionId { get; set; }

        // هل تم ربط الملف بطلب فعلاً؟
        public bool IsLinked { get; set; } = false;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("DesignFileId")]
        public virtual OrderItem? OrderItem { get; set; }
    }
}