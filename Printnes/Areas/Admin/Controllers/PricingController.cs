/* ============================================
 * الملف: Areas/Admin/Controllers/PricingController.cs
 * كنترولر إدارة مصفوفة الأسعار مع الفلترة
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
    public class PricingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PricingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? productIdFilter = null,
            int? paperFilter = null,
            string sortBy = "product")
        {
            var query = _context.PricingMatrices
                .Include(p => p.Product)
                .Include(p => p.PaperOption)
                .Include(p => p.SizeOption)
                .Include(p => p.SidesOption)
                .AsQueryable();

            if (productIdFilter.HasValue && productIdFilter.Value > 0)
                query = query.Where(p => p.ProductId == productIdFilter.Value);

            if (paperFilter.HasValue && paperFilter.Value > 0)
                query = query.Where(p => p.PaperOptionId == paperFilter.Value);

            query = sortBy switch
            {
                "price_high" => query.OrderByDescending(p => p.UnitPrice),
                "price_low" => query.OrderBy(p => p.UnitPrice),
                "base_high" => query.OrderByDescending(p => p.BasePrice),
                _ => query.OrderBy(p => p.Product.NameAr)
            };

            var pricingList = await query.ToListAsync();

            ViewBag.ProductIdFilter = productIdFilter;
            ViewBag.PaperFilter = paperFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.Products = new SelectList(
                await _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr).ToListAsync(),
                "Id", "NameAr");
            ViewBag.PaperOptions = new SelectList(
                await _context.ProductOptions.Where(o => o.OptionType == 1 && o.IsActive).OrderBy(o => o.NameAr).ToListAsync(),
                "Id", "NameAr");

            return View(pricingList);
        }

        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr), "Id", "NameAr");
            ViewData["PaperOptionId"] = new SelectList(
                _context.ProductOptions.Where(o => o.OptionType == 1 && o.IsActive), "Id", "NameAr");
            ViewData["SizeOptionId"] = new SelectList(
                _context.ProductOptions.Where(o => o.OptionType == 2 && o.IsActive), "Id", "NameAr");
            ViewData["SidesOptionId"] = new SelectList(
                _context.ProductOptions.Where(o => o.OptionType == 3 && o.IsActive), "Id", "NameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PricingMatrix matrix)
        {
            if (ModelState.IsValid)
            {
                // التحقق من عدم وجود التسعيرة مكررة لنفس التقاطع
                bool exists = await _context.PricingMatrices.AnyAsync(p =>
                    p.ProductId == matrix.ProductId &&
                    p.PaperOptionId == matrix.PaperOptionId &&
                    p.SizeOptionId == matrix.SizeOptionId &&
                    p.SidesOptionId == matrix.SidesOptionId);

                if (exists)
                {
                    ModelState.AddModelError("", "توجد تسعيرة لنفس التقاطع بالفعل. قم بتعديلها بدلاً من إضافة جديدة.");
                }
                else
                {
                    _context.Add(matrix);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم إضافة التسعيرة بنجاح!";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.IsActive), "Id", "NameAr", matrix.ProductId);
            ViewData["PaperOptionId"] = new SelectList(
                _context.ProductOptions.Where(o => o.OptionType == 1 && o.IsActive), "Id", "NameAr", matrix.PaperOptionId);
            ViewData["SizeOptionId"] = new SelectList(
                _context.ProductOptions.Where(o => o.OptionType == 2 && o.IsActive), "Id", "NameAr", matrix.SizeOptionId);
            ViewData["SidesOptionId"] = new SelectList(
                _context.ProductOptions.Where(o => o.OptionType == 3 && o.IsActive), "Id", "NameAr", matrix.SidesOptionId);
            return View(matrix);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var matrix = await _context.PricingMatrices.FindAsync(id);
            if (matrix != null)
            {
                _context.PricingMatrices.Remove(matrix);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف التسعيرة.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}