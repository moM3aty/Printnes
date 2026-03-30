/*
 * الملف: Areas/Admin/Controllers/UsersController.cs
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Printnes.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    // موديل مساعد لنقل بيانات الصلاحيات
    public class ManageUserRolesViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }

    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string searchQuery, string roleFilter, string statusFilter)
        {
            var query = _userManager.Users.AsQueryable();

            // الفلاتر
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(u => u.FullName.Contains(searchQuery) || u.Email.Contains(searchQuery) || u.PhoneNumber.Contains(searchQuery));
            }
            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "1";
                query = query.Where(u => u.IsActive == isActive);
            }

            var users = await query.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();

            int adminCount = 0;
            var finalUsers = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // فلتر الصلاحيات
                if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter))
                {
                    continue;
                }

                userRoles.Add(user.Id, roles);
                finalUsers.Add(user);

                if (roles.Contains("SuperAdmin") || roles.Contains("Admin")) adminCount++;
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.Roles = await _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToListAsync();

            // الإحصائيات
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            ViewBag.InactiveUsers = await _userManager.Users.CountAsync(u => !u.IsActive);
            ViewBag.AdminCount = adminCount;

            return View(finalUsers);
        }

        // =====================================
        // دوال إدارة صلاحيات المستخدم الجديدة
        // =====================================

        [HttpGet]
        public async Task<IActionResult> ManageRoles(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.UserId = id;
            ViewBag.UserName = user.FullName ?? user.UserName;

            var model = new List<ManageUserRolesViewModel>();
            var allRoles = await _roleManager.Roles.ToListAsync();

            foreach (var role in allRoles)
            {
                var userRoleViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                };
                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // حماية إضافية: منع الـ Admin العادي من سحب صلاحية SuperAdmin
            var currentUser = await _userManager.GetUserAsync(User);
            bool isCurrentUserSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            bool isTargetUserSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");

            if (isTargetUserSuperAdmin && !isCurrentUserSuperAdmin)
            {
                TempData["ErrorMessage"] = "ليس لديك الصلاحية لتعديل أدوار مدير نظام أعلى منك.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إزالة الصلاحيات القديمة.";
                return RedirectToAction(nameof(Index));
            }

            var selectedRoles = model.Where(x => x.IsSelected).Select(y => y.RoleName);
            result = await _userManager.AddToRolesAsync(user, selectedRoles);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إضافة الصلاحيات الجديدة.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"تم تحديث صلاحيات المستخدم ({user.FullName}) بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser.Id)
            {
                TempData["ErrorMessage"] = "لا يمكنك إيقاف حسابك الشخصي.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = user.IsActive ? "تم تفعيل الحساب." : "تم إيقاف الحساب.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (user.Id == currentUser.Id)
                {
                    TempData["ErrorMessage"] = "لا يمكنك حذف حسابك الشخصي!";
                    return RedirectToAction(nameof(Index));
                }
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "تم الحذف بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}