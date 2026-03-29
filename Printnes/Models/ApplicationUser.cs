using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Printnes.Models
{
    // توسيع كلاس المستخدم الافتراضي لإضافة بيانات إضافية لموظفي الـ ERP والعملاء
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;

        // يمكن إضافة حقول أخرى مثل (صورة الموظف، تاريخ التعيين، إلخ)
    }
}