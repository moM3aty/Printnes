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

        // عرض مصفوفة الأسعار كاملة
        public async Task<IActionResult> Index()
        {
            var pricingList = await _context.PricingMatrices
                .Include(p => p.Product)
                .Include(p => p.PaperOption)
                .Include(p => p.SizeOption)
                .Include(p => p.SidesOption)
                .ToListAsync();

            return View(pricingList);
        }

        // إضافة تسعيرة جديدة للتقاطعات (ورق + حجم + أوجه) لمنتج معين
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "NameAr");
            ViewData["PaperOptionId"] = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 1), "Id", "NameAr");
            ViewData["SizeOptionId"] = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 2), "Id", "NameAr");
            ViewData["SidesOptionId"] = new SelectList(_context.ProductOptions.Where(o => o.OptionType == 3), "Id", "NameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PricingMatrix matrix)
        {
            if (ModelState.IsValid)
            {
                _context.Add(matrix);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(matrix);
        }
    }
}