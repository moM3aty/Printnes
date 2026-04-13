/*
 * الملف: Models/CustomDesign.cs
 * الموديل المسؤول عن حفظ تصاميم العملاء المخصصة (JSON و صور)
 */
using System;
using System.ComponentModel.DataAnnotations;

namespace Printnes.Models
{
    public class CustomDesign
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; } // قد يكون العميل زائر (Guest)

        [Required]
        public string ProductType { get; set; } // مثال: box, tshirt, mug

        // أبعاد المنتج وقت التصميم
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string ColorHex { get; set; }

        // أهم حقل: حفظ التصميم بصيغة JSON من Fabric.js لتمكين التعديل اللاحق
        public string CanvasJsonData { get; set; }

        // رابط الصورة عالية الدقة التي تم تصديرها من الـ Canvas
        public string PreviewImageUrl { get; set; }

        public decimal EstimatedPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}