using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")] // الإعدادات للمدير العام فقط
    public class SettingsController : Controller
    {
        // عرض صفحة الإعدادات
        public IActionResult Index()
        {
            // في الواقع، يمكن تخزين هذه البيانات في جدول `Settings` في قاعدة البيانات
            // أو في ملف `appsettings.json`. هنا نقوم بإنشاء نموذج وهمي (Mock) للتصميم

            var settings = new ViewModels.StoreSettingsViewModel
            {
                StoreName = "مطابع برنتس (Printnes)",
                TaxPercentage = 15.0m,
                DefaultShippingCost = 25.0m,
                ContactEmail = "support@printnes.co",
                ContactPhone = "+966554804857"
            };

            return View(settings);
        }

        // حفظ التعديلات (يُنفذ لاحقاً بالربط مع الـ DbContext)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(ViewModels.StoreSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // حفظ التعديلات هنا
                TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            return View("Index", model);
        }
    }
}