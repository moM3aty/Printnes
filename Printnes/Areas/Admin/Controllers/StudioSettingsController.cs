using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Printnes.Helpers;
using Printnes.ViewModels;
using System.Linq;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class StudioSettingsController : Controller
    {
        public IActionResult Index()
        {
            var settings = StudioSettingsManager.LoadSettings();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveBaseCost(decimal basePrintCost)
        {
            var settings = StudioSettingsManager.LoadSettings();
            settings.BasePrintCost = basePrintCost;
            StudioSettingsManager.SaveSettings(settings);
            TempData["SuccessMessage"] = "تم تحديث التكلفة الأساسية بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMaterial(StudioMaterial material)
        {
            if (ModelState.IsValid)
            {
                var settings = StudioSettingsManager.LoadSettings();
                settings.Materials.Add(material);
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = "تم إضافة الخامة بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMaterial(string id)
        {
            var settings = StudioSettingsManager.LoadSettings();
            var item = settings.Materials.FirstOrDefault(m => m.Id == id);
            if (item != null)
            {
                settings.Materials.Remove(item);
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = "تم حذف الخامة.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSize(StudioSize size)
        {
            if (ModelState.IsValid)
            {
                var settings = StudioSettingsManager.LoadSettings();
                settings.Sizes.Add(size);
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = "تم إضافة المقاس.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSize(string id)
        {
            var settings = StudioSettingsManager.LoadSettings();
            var item = settings.Sizes.FirstOrDefault(s => s.Id == id);
            if (item != null)
            {
                settings.Sizes.Remove(item);
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = "تم حذف المقاس.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDiscountTier(StudioDiscountTier tier)
        {
            if (ModelState.IsValid)
            {
                var settings = StudioSettingsManager.LoadSettings();
                settings.DiscountTiers.Add(tier);
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = "تم إضافة شريحة الخصم.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDiscountTier(string id)
        {
            var settings = StudioSettingsManager.LoadSettings();
            var item = settings.DiscountTiers.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                settings.DiscountTiers.Remove(item);
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = "تم حذف الشريحة.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}