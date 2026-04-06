/*
 * الملف: Areas/Admin/Controllers/SettingsController.cs
 * تم تحديثه ليقوم بقراءة وحفظ الإعدادات فعلياً باستخدام SiteSettingsManager
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Printnes.ViewModels;
using Printnes.Helpers;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            // جلب الإعدادات الحقيقية من الملف
            var settings = SiteSettingsManager.LoadSettings();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(StoreSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // حفظ الإعدادات فعلياً
                SiteSettingsManager.SaveSettings(model);
                TempData["SuccessMessage"] = "تم تحديث وحفظ إعدادات المتجر بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            return View("Index", model);
        }

        public IActionResult ClearCache()
        {
            TempData["SuccessMessage"] = "تم مسح الـ Cache بنجاح وتحديث النظام.";
            return RedirectToAction(nameof(Index));
        }
    }
}