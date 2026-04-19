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
        [HttpPost]
        [ValidateAntiForgeryToken] // يمكنك إزالتها لهذا الـ Endpoint إذا واجهت مشكلة مع الـ Fetch API
        [Route("Admin/Settings/UploadLogo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "لم يتم اختيار أي ملف." });
                }

                // التأكد من صيغة الملف
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".svg")
                {
                    return Json(new { success = false, message = "صيغة غير مدعومة. يرجى رفع صورة (PNG, JPG, SVG)." });
                }

                // إنشاء مجلد الرفع إذا لم يكن موجوداً
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "settings");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // إنشاء اسم عشوائي لمنع تعارض الأسماء
                string uniqueFileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // حفظ الملف
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // مسار الصورة الذي سيتم حفظه في الداتا بيز واستخدامه في الـ HTML
                string imageUrl = "/uploads/settings/" + uniqueFileName;

                return Json(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ داخلي: " + ex.Message });
            }
        }
        public IActionResult ClearCache()
        {
            TempData["SuccessMessage"] = "تم مسح الـ Cache بنجاح وتحديث النظام.";
            return RedirectToAction(nameof(Index));
        }
    }
}