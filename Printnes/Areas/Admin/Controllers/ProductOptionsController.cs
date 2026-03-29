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

        // عرض جميع خيارات المنتجات
        public async Task<IActionResult> Index()
        {
            var options = await _context.ProductOptions.Include(o => o.Product).ToListAsync();
            return View(options);
        }

        // إضافة خيار جديد
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "NameAr");
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "NameAr", option.ProductId);
            return View(option);
        }
    }
}