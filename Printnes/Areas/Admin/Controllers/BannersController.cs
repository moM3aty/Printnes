/* ============================================
 * الملف: Areas/Admin/Controllers/BannersController.cs
 * كنترولر إدارة البانرات مع رفع الملفات
 * ============================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class BannersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BannersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banners.OrderBy(b => b.SortOrder).ToListAsync();
            return View(banners);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // التحقق من نوع الملف
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageUrl", "صيغة الملف غير مدعومة. استخدم JPG, PNG, WebP أو GIF");
                        return View(banner);
                    }

                    // التحقق من الحجم (10MB كحد أقصى)
                    if (imageFile.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageUrl", "حجم الملف يتجاوز 10MB");
                        return View(banner);
                    }

                    banner.ImageUrl = await UploadFileAsync(imageFile, "banners");
                }
                else
                {
                    ModelState.AddModelError("ImageUrl", "يجب رفع صورة للبانر");
                    return View(banner);
                }

                _context.Add(banner);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم رفع البانر بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                if (!string.IsNullOrEmpty(banner.ImageUrl))
                {
                    DeleteFile(banner.ImageUrl);
                }

                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف البانر بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                banner.IsActive = !banner.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = banner.IsActive ? "تم تفعيل البانر." : "تم إخفاء البانر.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + folderName + "/" + uniqueFileName;
        }

        private void DeleteFile(string fileUrl)
        {
            if (!string.IsNullOrEmpty(fileUrl))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, fileUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
        }
    }
}