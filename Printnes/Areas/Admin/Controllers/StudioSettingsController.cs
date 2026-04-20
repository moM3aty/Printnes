using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Printnes.Helpers;
using Printnes.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class StudioSettingsController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        // حقن بيئة الاستضافة لمعرفة مسار حفظ الصور
        public StudioSettingsController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

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
        public IActionResult ToggleBoxType(string id)
        {
            var settings = StudioSettingsManager.LoadSettings();
            var box = settings.BoxTypes.FirstOrDefault(b => b.Id == id);
            if (box != null)
            {
                box.IsActive = !box.IsActive;
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = box.IsActive ? $"تم تفعيل {box.Name}" : $"تم إخفاء {box.Name} من الاستوديو";
            }
            return RedirectToAction(nameof(Index));
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBoxType(string id)
        {
            var settings = StudioSettingsManager.LoadSettings();
            var item = settings.BoxTypes.FirstOrDefault(b => b.Id == id);
            if (item != null)
            {
                // مسح الصورة من السيرفر إذا لم تكن الصورة الافتراضية
                if (!string.IsNullOrEmpty(item.ImagePath) && item.ImagePath.Contains("/uploads/boxes/"))
                {
                    var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string imagePath = Path.Combine(webRootPath, item.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                settings.BoxTypes.Remove(item);
                StudioSettingsManager.SaveSettings(settings);
                TempData["SuccessMessage"] = "تم حذف العلبة.";
            }
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
        public async Task<IActionResult> AddBoxType(StudioBoxType box, IFormFile boxImage)
        {
            if (ModelState.IsValid)
            {
                var settings = StudioSettingsManager.LoadSettings();

               
                    
                        var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRootPath, "uploads", "boxes");

                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(boxImage.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await boxImage.CopyToAsync(fileStream);
                        }

                        // حفظ المسار الجديد
                        box.ImagePath = "/uploads/boxes/" + uniqueFileName;
                    

                    box.IsActive = true;
                    settings.BoxTypes.Add(box);
                    StudioSettingsManager.SaveSettings(settings);
                    TempData["SuccessMessage"] = "تم إضافة العلبة بنجاح.";
                
                
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