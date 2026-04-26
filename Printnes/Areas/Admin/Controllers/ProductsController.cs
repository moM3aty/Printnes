/* ============================================
 * الملف: Areas/Admin/Controllers/ProductsController.cs
 * كنترولر إدارة المنتجات مع رفع الملفات والفلترة
 * ============================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(
            string searchQuery = "",
            int? categoryId = null,
            byte? status = null,
            string sortBy = "newest")
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p =>
                    p.NameAr.Contains(searchQuery) ||
                    p.NameEn.Contains(searchQuery.ToLower()) ||
                    p.Slug.Contains(searchQuery.ToLower()));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (status.HasValue)
                query = query.Where(p => p.IsActive == (status.Value == 1));

            query = sortBy switch
            {
                "oldest" => query.OrderBy(p => p.Id),
                "name_asc" => query.OrderBy(p => p.NameAr),
                "name_desc" => query.OrderByDescending(p => p.NameAr),
                _ => query.OrderByDescending(p => p.Id)
            };

            var products = await query.ToListAsync();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.CategoryId = categoryId;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.NameAr).ToListAsync(),
                "Id", "NameAr");

            return View(products);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.IsActive).OrderBy(c => c.NameAr), "Id", "NameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile coverImageFile)
        {
            ModelState.Remove("CoverImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    // التحقق من عدم تكرار الـ Slug
                    bool slugExists = await _context.Products.AnyAsync(p => p.Slug == product.Slug);
                    if (slugExists)
                    {
                        ModelState.AddModelError("Slug", "هذا الرابط (Slug) مستخدم بالفعل لمنتج آخر، يرجى اختيار رابط مختلف.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "NameAr", product.CategoryId);
                        return View(product);
                    }

                    if (coverImageFile != null && coverImageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                        var ext = Path.GetExtension(coverImageFile.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(ext))
                        {
                            ModelState.AddModelError("CoverImageUrl", "صيغة الصورة غير مدعومة. استخدم JPG, PNG أو WebP");
                            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "NameAr", product.CategoryId);
                            return View(product);
                        }

                        product.CoverImageUrl = await UploadFileAsync(coverImageFile, "products");
                    }
                    else
                    {
                        ModelState.AddModelError("CoverImageUrl", "يجب رفع صورة للمنتج");
                        ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "NameAr", product.CategoryId);
                        return View(product);
                    }

                    product.DescriptionAr = product.DescriptionAr ?? "";
                    product.DescriptionEn = product.DescriptionEn ?? "";

                    product.CreatedAt = DateTime.UtcNow;
                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ModelState.AddModelError("", "حدث خطأ أثناء الحفظ: " + errorMsg);
                }
            }

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.IsActive), "Id", "NameAr", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.IsActive).OrderBy(c => c.NameAr), "Id", "NameAr", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile coverImageFile)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("CoverImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    bool slugExists = await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.Id != product.Id);
                    if (slugExists)
                    {
                        ModelState.AddModelError("Slug", "هذا الرابط (Slug) مستخدم بالفعل لمنتج آخر، يرجى اختيار رابط مختلف.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "NameAr", product.CategoryId);
                        return View(product);
                    }

                    if (coverImageFile != null && coverImageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(product.CoverImageUrl))
                            DeleteFile(product.CoverImageUrl);

                        product.CoverImageUrl = await UploadFileAsync(coverImageFile, "products");
                    }

                    product.DescriptionAr = product.DescriptionAr ?? "";
                    product.DescriptionEn = product.DescriptionEn ?? "";

                    product.UpdatedAt = DateTime.UtcNow;
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم تعديل المنتج بنجاح!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ModelState.AddModelError("", "حدث خطأ أثناء التعديل: " + errorMsg);
                }
            }

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.IsActive), "Id", "NameAr", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = product.IsActive ? "تم تفعيل المنتج." : "تم تعطيل المنتج.";
            }
            return RedirectToAction(nameof(Index));
        }

        // 🟢 التعديل الجذري: الحذف الإجباري (Force Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // بدء عملية Transaction لضمان الحذف الآمن للبيانات المرتبطة
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    // 1. مسح مصفوفة التسعير (الأسعار المخصصة) المرتبطة بالمنتج
                    var pricing = await _context.PricingMatrices.Where(p => p.ProductId == id).ToListAsync();
                    _context.PricingMatrices.RemoveRange(pricing);

                    // 2. مسح الخيارات المرتبطة (المقاسات، أنواع الورق، التغليف)
                    var options = await _context.ProductOptions.Where(o => o.ProductId == id).ToListAsync();
                    _context.ProductOptions.RemoveRange(options);

                    // 3. مسح الإضافات الاختيارية المرتبطة بالمنتج
                    var extras = await _context.ProductExtras.Where(e => e.ProductId == id).ToListAsync();
                    _context.ProductExtras.RemoveRange(extras);

                    // 4. مسح شرائح الكميات (الخصومات الجملة)
                    var tiers = await _context.QuantityTiers.Where(q => q.ProductId == id).ToListAsync();
                    _context.QuantityTiers.RemoveRange(tiers);

                    // 5. مسح التقييمات المرتبطة بالمنتج
                    var reviews = await _context.ProductReviews.Where(r => r.ProductId == id).ToListAsync();
                    _context.ProductReviews.RemoveRange(reviews);

                    // 6. مسح المنتج من مفضلات العملاء
                    var favs = await _context.UserFavorites.Where(f => f.ProductId == id).ToListAsync();
                    _context.UserFavorites.RemoveRange(favs);

                    // 7. فك ارتباط المنتج بالطلبات السابقة (حتى لا يتم حذف فواتير العملاء القديمة من المتجر)
                    var orderItems = await _context.OrderItems.Where(oi => oi.ProductId == id).ToListAsync();
                    foreach (var item in orderItems)
                    {
                        item.ProductId = null;
                    }

                    // حفظ مسح البيانات المرتبطة أولاً
                    await _context.SaveChangesAsync();

                    // 8. حذف صورة المنتج من السيرفر لتوفير المساحة
                    if (!string.IsNullOrEmpty(product.CoverImageUrl))
                        DeleteFile(product.CoverImageUrl);

                    // 9. أخيراً، حذف المنتج نفسه
                    _context.Products.Remove(product);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "تم حذف المنتج وكافة بياناته المرتبطة بنجاح.";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "حدث خطأ أثناء الحذف الإجباري: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            string uploadsFolder = Path.Combine(webRootPath, "uploads", folderName);
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
                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string imagePath = Path.Combine(webRootPath, fileUrl.TrimStart('/'));

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }
        }
    }
}