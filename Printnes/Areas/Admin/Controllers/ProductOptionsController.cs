/* ============================================
 * الملف: Areas/Admin/Controllers/ProductOptionsController.cs
 * كنترولر إدارة خيارات المنتجات مع الفلترة
 * ============================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ProductOptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductOptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? productIdFilter = null,
            byte? optionTypeFilter = null,
            byte? statusFilter = null,
            string searchQuery = "",
            string sortBy = "product")
        {
            var query = _context.ProductOptions.Include(o => o.Product).AsQueryable();

            if (productIdFilter.HasValue && productIdFilter.Value > 0)
                query = query.Where(o => o.ProductId == productIdFilter.Value);

            if (optionTypeFilter.HasValue && optionTypeFilter.Value > 0)
                query = query.Where(o => o.OptionType == optionTypeFilter.Value);

            if (statusFilter.HasValue)
                query = query.Where(o => o.IsActive == (statusFilter.Value == 1));

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(o =>
                    o.NameAr.Contains(searchQuery) ||
                    o.NameEn.ToLower().Contains(searchQuery.ToLower()));
            }

            query = sortBy switch
            {
                "name_desc" => query.OrderByDescending(o => o.NameAr),
                "type" => query.OrderBy(o => o.OptionType).ThenBy(o => o.NameAr),
                _ => query.OrderBy(o => o.Product.NameAr).ThenBy(o => o.OptionType).ThenBy(o => o.SortOrder)
            };

            var options = await query.ToListAsync();

            ViewBag.ProductIdFilter = productIdFilter;
            ViewBag.OptionTypeFilter = optionTypeFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortBy = sortBy;
            ViewBag.Products = new SelectList(
                await _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr).ToListAsync(),
                "Id", "NameAr");

            return View(options);
        }

        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr), "Id", "NameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductOption option)
        {
            if (ModelState.IsValid)
            {
                _context.Add(option);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة الخيار بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive), "Id", "NameAr", option.ProductId);
            return View(option);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var option = await _context.ProductOptions.FindAsync(id);
            if (option == null) return NotFound();

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr), "Id", "NameAr", option.ProductId);
            return View(option);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductOption option)
        {
            if (id != option.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(option);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل الخيار بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive), "Id", "NameAr", option.ProductId);
            return View(option);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var option = await _context.ProductOptions.FindAsync(id);
            if (option != null)
            {
                option.IsActive = !option.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = option.IsActive ? "تم تفعيل الخيار." : "تم تعطيل الخيار.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var option = await _context.ProductOptions.FindAsync(id);
            if (option != null)
            {
                // التحقق من عدم وجود تسعيرات مرتبطة
                bool hasPricing = await _context.PricingMatrices.AnyAsync(p =>
                    p.PaperOptionId == id || p.SizeOptionId == id || p.SidesOptionId == id);

                if (hasPricing)
                {
                    TempData["ErrorMessage"] = "لا يمكن حذف هذا الخيار لوجود تسعيرات مرتبطة به. قم بحذف التسعيرات أولاً.";
                    return RedirectToAction(nameof(Index));
                }

                _context.ProductOptions.Remove(option);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الخيار بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}