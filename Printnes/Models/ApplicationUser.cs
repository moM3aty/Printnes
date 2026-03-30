/* ============================================
 * الملف: Models/ApplicationUser.cs
 * موديل المستخدم الموسع - يرث من IdentityUser
 * يضيف حقول إضافية: FullName, IsActive, PhoneNumber
 * يُستخدم لكلاً من الأدمن والعملاء
 * ============================================ */

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Printnes.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(200, ErrorMessage = "الاسم الكريم مطلوب وأقصى 200 حرف")]
        public string FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLogin { get; set; }

        // يمكن إضافة حقول أخرى مثل: Address, City, CompanyName لاحقاً
    }
}