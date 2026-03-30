/*
 * الملف: Areas/Admin/Controllers/RolesController.cs
 * تم تحسين معالجة الأخطاء هنا لتجنب صفحة 404، بحيث يتم إرجاعك لصفحة الصلاحيات مع رسالة خطأ إذا كان هناك مشكلة.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "اسم الصلاحية مطلوب.");
                return View();
            }

            roleName = roleName.Trim();

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"تم إنشاء الصلاحية '{roleName}' بنجاح.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            else
            {
                ModelState.AddModelError("", "هذه الصلاحية موجودة بالفعل.");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "المعرف غير صحيح.";
                return RedirectToAction(nameof(Index));
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                // بدلاً من إرجاع 404 NotFound، نعيدك للصفحة مع رسالة
                TempData["ErrorMessage"] = "لم يتم العثور على الصلاحية المطلوبة.";
                return RedirectToAction(nameof(Index));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            var allUsers = await _userManager.Users.ToListAsync();

            var viewModel = new ViewModels.UserRolesViewModel
            {
                UserId = id,
                FullName = role.Name,
                Email = role.Name, // هنا نستخدمها كاسم مؤقت
                Roles = allUsers.Select(u => new ViewModels.RoleSelection
                {
                    UserId = u.Id,
                    RoleName = role.Name,
                    IsSelected = usersInRole.Any(x => x.Id == u.Id)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ViewModels.UserRolesViewModel model)
        {
            if (model.UserId == null)
            {
                TempData["ErrorMessage"] = "حدث خطأ في معرف الصلاحية.";
                return RedirectToAction(nameof(Index));
            }

            var role = await _roleManager.FindByIdAsync(model.UserId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على الصلاحية.";
                return RedirectToAction(nameof(Index));
            }

            var usersToAdd = new List<string>();
            var usersToRemove = new List<string>();

            if (model.Roles != null)
            {
                foreach (var r in model.Roles)
                {
                    var userForCheck = await _userManager.FindByIdAsync(r.UserId);
                    if (userForCheck != null)
                    {
                        bool isInRole = await _userManager.IsInRoleAsync(userForCheck, role.Name);

                        if (r.IsSelected && !isInRole)
                        {
                            usersToAdd.Add(r.UserId);
                        }
                        else if (!r.IsSelected && isInRole)
                        {
                            usersToRemove.Add(r.UserId);
                        }
                    }
                }
            }

            foreach (var userId in usersToAdd)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    await _userManager.AddToRoleAsync(user, role.Name);
            }

            foreach (var userId in usersToRemove)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
            }

            TempData["SuccessMessage"] = $"تم تحديث مستخدمي صلاحية '{role.Name}' بنجاح. (إضافة {usersToAdd.Count}، إزالة {usersToRemove.Count})";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

            var role = await _roleManager.FindByIdAsync(id);

            var protectedRoles = new[] { "SuperAdmin", "Admin", "Customer", "Accountant" };
            if (role != null && protectedRoles.Contains(role.Name))
            {
                TempData["ErrorMessage"] = $"لا يمكن حذف الصلاحية المحمية: {role.Name}";
                return RedirectToAction(nameof(Index));
            }

            if (role != null)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                foreach (var user in usersInRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }

                await _roleManager.DeleteAsync(role);
                TempData["SuccessMessage"] = $"تم حذف صلاحية '{role.Name}' بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}