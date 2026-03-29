using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
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

        // GET: /Admin/ProductExtras
        public async Task<IActionResult> Index()
        {
            var extras = await _context.ProductExtras.Include(p => p.Product).ToListAsync();
            return View(extras);
        }

        // GET: /Admin/ProductExtras/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "NameAr");
            return View();
        }

        // POST: /Admin/ProductExtras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductExtra productExtra)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productExtra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "NameAr", productExtra.ProductId);
            return View(productExtra);
        }
    }
}