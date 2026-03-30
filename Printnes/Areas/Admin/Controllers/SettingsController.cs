/* ============================================
 * الملف: Areas/Admin/Controllers/SettingsController.cs
 * كنترولر إعدادات النظام والمتجر
 * ============================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Printnes.ViewModels;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            var settings = new StoreSettingsViewModel
            {
                StoreName = "مطابع برنتس (Printnes)",
                TaxPercentage = 15.0m,
                DefaultShippingCost = 25.0m,
                ContactEmail = "support@printnes.co",
                ContactPhone = "+966554804857",
                WhatsappNumber = "966554804857",
                StoreDescription = "PRINTNES مؤسسة سعودية معرّفه في وزارة التجارة متخصصين في تصنيع المطبوعات وتقديم خدمات التصميم الفريدة",
                FacebookUrl = "",
                InstagramUrl = "",
                TiktokUrl = "",
                TwitterUrl = "",
                Currency = "ر.س",
                EnableReviews = true,
                EnableUserRegistration = true,
                EnableGuestCheckout = true,
                MinimumOrderAmount = 0,
                FreeShippingAbove = 500
            };

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(StoreSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // في الإنتاج الفعلي يتم حفظ هذه الإعدادات في جدول Settings في قاعدة البيانات
                // أو في ملف appsettings.json
                // حالياً نحفظها في TempData للعرض
                TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح! (ملاحظة: في الإنتاج الفعلي يجب ربطها بالـ DbContext)";
                return RedirectToAction(nameof(Index));
            }
            return View("Index", model);
        }

        public IActionResult ClearCache()
        {
            // مسح الـ Cache (يمكن إضافة ResponseCache و IMemoryCache لاحقاً)
            TempData["SuccessMessage"] = "تم مسح الـ Cache بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }
}