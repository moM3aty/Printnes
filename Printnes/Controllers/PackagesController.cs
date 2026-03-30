/*
 * الملف: Controllers/PackagesController.cs
 * تم تحديثه لجلب خيارات الطباعة من قاعدة البيانات وتمريرها لواجهة البكجات
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
    public class PackagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PackagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("Packages")]
        public async Task<IActionResult> Index()
        {
            var packages = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .Take(12)
                .ToListAsync();

            return View(packages);
        }

        [Route("Packages/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductExtras.Where(e => e.IsActive))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            // تمرير خيارات الطباعة الخاصة بالبكج
            ViewBag.PaperOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 1 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.SizeOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 2 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.SidesOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 3 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.FinishOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 4 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.QuantityTiers = await _context.QuantityTiers.Where(q => q.ProductId == id).OrderBy(q => q.MinQuantity).ToListAsync();

            return View(product);
        }
    }
}