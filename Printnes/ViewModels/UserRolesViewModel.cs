using System.Collections.Generic;

namespace Printnes.ViewModels
{
    // هذا الكلاس يستخدم في شاشة تعيين الصلاحيات (Roles) للمستخدمين والموظفين
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // قائمة بجميع الصلاحيات المتوفرة في النظام مع حالة اختيارها للمستخدم الحالي
        public List<RoleSelection> Roles { get; set; }
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}