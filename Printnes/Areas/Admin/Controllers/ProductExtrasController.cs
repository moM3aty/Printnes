/* ============================================
 * الملف: Areas/Admin/Controllers/ProductExtrasController.cs
 * كنترولر إدارة الإضافات الاختيارية للمنتجات
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
    public class ProductExtrasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductExtrasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? productIdFilter = null,
            byte? statusFilter = null,
            string sortBy = "name")
        {
            var query = _context.ProductExtras.Include(p => p.Product).AsQueryable();

            if (productIdFilter.HasValue && productIdFilter.Value > 0)
                query = query.Where(p => p.ProductId == productIdFilter.Value);

            if (statusFilter.HasValue)
                query = query.Where(p => p.IsActive == (statusFilter.Value == 1));

            query = sortBy switch
            {
                "price_high" => query.OrderByDescending(p => p.Price),
                "price_low" => query.OrderBy(p => p.Price),
                "name_desc" => query.OrderByDescending(p => p.NameAr),
                _ => query.OrderBy(p => p.NameAr)
            };

            var extras = await query.ToListAsync();

            ViewBag.ProductIdFilter = productIdFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.Products = new SelectList(
                await _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr).ToListAsync(),
                "Id", "NameAr");

            return View(extras);
        }

        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr), "Id", "NameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductExtra productExtra)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productExtra);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة الخيار الإضافي بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive), "Id", "NameAr", productExtra.ProductId);
            return View(productExtra);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var extra = await _context.ProductExtras.FindAsync(id);
            if (extra == null) return NotFound();

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr), "Id", "NameAr", extra.ProductId);
            return View(extra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductExtra productExtra)
        {
            if (id != productExtra.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(productExtra);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل الخيار الإضافي بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive), "Id", "NameAr", productExtra.ProductId);
            return View(productExtra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var extra = await _context.ProductExtras.FindAsync(id);
            if (extra != null)
            {
                extra.IsActive = !extra.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = extra.IsActive ? "تم تفعيل الخيار الإضافي." : "تم تعطيل الخيار الإضافي.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var extra = await _context.ProductExtras.FindAsync(id);
            if (extra != null)
            {
                _context.ProductExtras.Remove(extra);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الخيار الإضافي.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}