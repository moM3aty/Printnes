/*
 * الملف: Areas/Admin/Controllers/UsersController.cs
 * تم التأكد من وجود دوال الـ Edit لتعمل واجهة تعديل المستخدمين بشكل سليم
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Printnes.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchQuery = "", string roleFilter = "", byte? statusFilter = null, string sortBy = "newest")
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(u =>
                    u.FullName.Contains(searchQuery) ||
                    u.Email.Contains(searchQuery) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchQuery)));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                var userIdsInRole = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            if (statusFilter.HasValue)
            {
                if (statusFilter.Value == 1)
                    query = query.Where(u => u.IsActive);
                else
                    query = query.Where(u => !u.IsActive);
            }

            query = sortBy switch
            {
                "name_desc" => query.OrderByDescending(u => u.FullName),
                "name_asc" => query.OrderBy(u => u.FullName),
                "email" => query.OrderBy(u => u.Email),
                _ => query.OrderByDescending(u => u.Id)
            };

            var users = await query.ToListAsync();

            var userRolesDict = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRolesDict;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.Roles = new SelectList(await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync(), "Name", "Name");

            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.IsActive);
            ViewBag.InactiveUsers = users.Count(u => !u.IsActive);
            ViewBag.AdminCount = users.Count(u => userRolesDict.ContainsKey(u.Id) && (userRolesDict[u.Id].Contains("SuperAdmin") || userRolesDict[u.Id].Contains("Admin")));

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin"))
                {
                    TempData["ErrorMessage"] = "لا يمكن تعطيل حساب المدير العام.";
                    return RedirectToAction(nameof(Index));
                }

                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = user.IsActive ? "تم تفعيل المستخدم." : "تم تعطيل المستخدم.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ====== دالة عرض واجهة التعديل ======
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // ====== دالة حفظ التعديلات ======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("SuperAdmin"))
            {
                user.IsActive = model.IsActive;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "تم تعديل بيانات المستخدم بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin"))
            {
                TempData["ErrorMessage"] = "لا يمكن حذف حساب المدير العام.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                TempData["SuccessMessage"] = $"تم حذف المستخدم '{user.FullName}' بنجاح.";
            else
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المستخدم.";

            return RedirectToAction(nameof(Index));
        }
    }
}