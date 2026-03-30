/*
 * الملف: Areas/Admin/Controllers/PricingController.cs
 * يحتوي على جميع دوال مصفوفة التسعير بما فيها دوال الـ Edit المطلوبة.
 */

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
    public class PricingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PricingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Pricing
        public async Task<IActionResult> Index()
        {
            var pricingMatrices = await _context.PricingMatrices
                .Include(p => p.Product)
                .Include(p => p.PaperOption)
                .Include(p => p.SizeOption)
                .Include(p => p.SidesOption)
                .ToListAsync();

            return View(pricingMatrices);
        }

        // GET: Admin/Pricing/Create
        public IActionResult Create()
        {
            ViewBag.ProductId = new SelectList(_context.Products, "Id", "NameAr");
            ViewBag.PaperOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 1), "Id", "NameAr");
            ViewBag.SizeOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 2), "Id", "NameAr");
            ViewBag.SidesOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 3), "Id", "NameAr");

            return View();
        }

        // POST: Admin/Pricing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PricingMatrix matrix)
        {
            if (ModelState.IsValid)
            {
                // التحقق من عدم وجود نفس التقاطع مسبقاً لنفس المنتج
                bool exists = await _context.PricingMatrices.AnyAsync(m =>
                    m.ProductId == matrix.ProductId &&
                    m.PaperOptionId == matrix.PaperOptionId &&
                    m.SizeOptionId == matrix.SizeOptionId &&
                    m.SidesOptionId == matrix.SidesOptionId);

                if (exists)
                {
                    ModelState.AddModelError("", "هذا التقاطع (نفس الخيارات) مسجل مسبقاً لهذا المنتج.");
                }
                else
                {
                    _context.PricingMatrices.Add(matrix);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تمت إضافة التسعيرة بنجاح.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.ProductId = new SelectList(_context.Products, "Id", "NameAr", matrix.ProductId);
            ViewBag.PaperOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 1), "Id", "NameAr", matrix.PaperOptionId);
            ViewBag.SizeOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 2), "Id", "NameAr", matrix.SizeOptionId);
            ViewBag.SidesOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 3), "Id", "NameAr", matrix.SidesOptionId);

            return View(matrix);
        }

        // ==========================================
        // دوال التعديل (Edit) التي طلبتها
        // ==========================================

        // GET: Admin/Pricing/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var matrix = await _context.PricingMatrices.FindAsync(id);
            if (matrix == null) return NotFound();

            // تمرير القوائم المنسدلة للواجهة مع تحديد القيم الحالية
            ViewBag.ProductId = new SelectList(_context.Products, "Id", "NameAr", matrix.ProductId);
            ViewBag.PaperOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 1), "Id", "NameAr", matrix.PaperOptionId);
            ViewBag.SizeOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 2), "Id", "NameAr", matrix.SizeOptionId);
            ViewBag.SidesOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 3), "Id", "NameAr", matrix.SidesOptionId);

            return View(matrix);
        }

        // POST: Admin/Pricing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PricingMatrix matrix)
        {
            if (id != matrix.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // التحقق من عدم وجود تقاطع مطابق آخر (غير الحالي)
                    bool exists = await _context.PricingMatrices.AnyAsync(m =>
                        m.Id != matrix.Id &&
                        m.ProductId == matrix.ProductId &&
                        m.PaperOptionId == matrix.PaperOptionId &&
                        m.SizeOptionId == matrix.SizeOptionId &&
                        m.SidesOptionId == matrix.SidesOptionId);

                    if (exists)
                    {
                        ModelState.AddModelError("", "يوجد تسعيرة أخرى بنفس هذه المواصفات تماماً.");
                    }
                    else
                    {
                        _context.Update(matrix);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "تم تعديل التسعيرة بنجاح.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PricingMatrixExists(matrix.Id)) return NotFound();
                    else throw;
                }
            }

            ViewBag.ProductId = new SelectList(_context.Products, "Id", "NameAr", matrix.ProductId);
            ViewBag.PaperOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 1), "Id", "NameAr", matrix.PaperOptionId);
            ViewBag.SizeOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 2), "Id", "NameAr", matrix.SizeOptionId);
            ViewBag.SidesOptionId = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 3), "Id", "NameAr", matrix.SidesOptionId);

            return View(matrix);
        }

        // POST: Admin/Pricing/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var matrix = await _context.PricingMatrices.FindAsync(id);
            if (matrix != null)
            {
                _context.PricingMatrices.Remove(matrix);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف التسعيرة بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PricingMatrixExists(int id)
        {
            return _context.PricingMatrices.Any(e => e.Id == id);
        }
    }
}