/* ============================================
 * الملف: Areas/Admin/Controllers/QuantityTiersController.cs
 * كنترولر إدارة شرائح خصم الكميات مع الفلترة
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
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public class QuantityTiersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuantityTiersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? productIdFilter = null,
            string sortBy = "quantity")
        {
            var query = _context.QuantityTiers.Include(q => q.Product).AsQueryable();

            if (productIdFilter.HasValue && productIdFilter.Value > 0)
                query = query.Where(q => q.ProductId == productIdFilter.Value);

            query = sortBy switch
            {
                "discount_high" => query.OrderByDescending(q => q.DiscountPercent),
                "discount_low" => query.OrderBy(q => q.DiscountPercent),
                "quantity_high" => query.OrderByDescending(q => q.MinQuantity),
                _ => query.OrderBy(q => q.Product.NameAr).ThenBy(q => q.MinQuantity)
            };

            var tiers = await query.ToListAsync();

            ViewBag.ProductIdFilter = productIdFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.Products = new SelectList(
                await _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr).ToListAsync(),
                "Id", "NameAr");

            return View(tiers);
        }

        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr), "Id", "NameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuantityTier quantityTier)
        {
            if (ModelState.IsValid)
            {
                // التحقق من عدم تعارض الشرائح
                bool overlap = await _context.QuantityTiers.AnyAsync(q =>
                    q.ProductId == quantityTier.ProductId &&
                    ((quantityTier.MinQuantity >= q.MinQuantity && quantityTier.MinQuantity <= q.MaxQuantity) ||
                     (quantityTier.MaxQuantity >= q.MinQuantity && quantityTier.MaxQuantity <= q.MaxQuantity) ||
                     (quantityTier.MinQuantity <= q.MinQuantity && quantityTier.MaxQuantity >= q.MaxQuantity)));

                if (overlap)
                {
                    ModelState.AddModelError("", "هذه الشريحة تتعارض مع شريحة موجودة مسبقاً لنفس المنتج.");
                }
                else
                {
                    _context.Add(quantityTier);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم إضافة شريحة الخصم بنجاح!";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive), "Id", "NameAr", quantityTier.ProductId);
            return View(quantityTier);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tier = await _context.QuantityTiers.FindAsync(id);
            if (tier == null) return NotFound();

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr), "Id", "NameAr", tier.ProductId);
            return View(tier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuantityTier quantityTier)
        {
            if (id != quantityTier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(quantityTier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل شريحة الخصم بنجاح!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive), "Id", "NameAr", quantityTier.ProductId);
            return View(quantityTier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tier = await _context.QuantityTiers.FindAsync(id);
            if (tier != null)
            {
                _context.QuantityTiers.Remove(tier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف شريحة الخصم.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}