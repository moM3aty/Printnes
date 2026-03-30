/* ============================================
 * الملف: Areas/Admin/Controllers/CategoriesController.cs
 * كنترولر إدارة الأقسام مع رفع الصور والفلترة
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
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(
            string searchQuery = "",
            string parentFilter = "",
            byte? status = null,
            string sortBy = "sortOrder")
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(c =>
                    c.NameAr.Contains(searchQuery) ||
                    c.NameEn.Contains(searchQuery) ||
                    c.Slug.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(parentFilter))
            {
                query = query.Where(c => c.ParentMenu == parentFilter);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.IsActive == (status.Value == 1));
            }

            query = sortBy switch
            {
                "name_asc" => query.OrderBy(c => c.NameAr),
                "name_desc" => query.OrderByDescending(c => c.NameAr),
                "newest" => query.OrderByDescending(c => c.Id),
                _ => query.OrderBy(c => c.SortOrder)
            };

            var categories = await query.ToListAsync();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.ParentFilter = parentFilter;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.ParentMenus = new[]
            {
                new { Value = "", Text = "الكل" },
                new { Value = "Stickers", Text = "طباعة الملصقات والاستيكرات" },
                new { Value = "PaperPrints", Text = "مطبوعات ورقية" },
                new { Value = "Boxes", Text = "الطباعة على البوكسات" },
                new { Value = "GiftCards", Text = "بطاقات الشكر والاهداء" },
                new { Value = "Packaging", Text = "التغليف والتعبئة" }
            };

            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    category.ImageUrl = await UploadFileAsync(imageFile, "categories");
                }

                category.CreatedAt = DateTime.UtcNow;
                _context.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إضافة القسم بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile imageFile)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        DeleteFile(category.ImageUrl);
                    }
                    category.ImageUrl = await UploadFileAsync(imageFile, "categories");
                }

                _context.Update(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تعديل القسم بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                {
                    TempData["ErrorMessage"] = "لا يمكن حذف هذا القسم لوجود منتجات مرتبطة به.";
                    return RedirectToAction(nameof(Index));
                }

                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    DeleteFile(category.ImageUrl);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف القسم بنجاح.";
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