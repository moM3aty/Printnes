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
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public class QuantityTiersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuantityTiersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/QuantityTiers
        public async Task<IActionResult> Index()
        {
            var tiers = await _context.QuantityTiers.Include(q => q.Product).ToListAsync();
            return View(tiers);
        }

        // GET: /Admin/QuantityTiers/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "NameAr");
            return View();
        }

        // POST: /Admin/QuantityTiers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuantityTier quantityTier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quantityTier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "NameAr", quantityTier.ProductId);
            return View(quantityTier);
        }
    }
}