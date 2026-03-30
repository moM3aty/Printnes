/*
 * الملف: Controllers/ProductsController.cs
 * تم إضافة سطر Include الخاص بجلب التقييمات المعتمدة (ProductReviews)
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // صفحة كل المنتجات
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        // صفحة تفاصيل المنتج (التي كان بها المشكلة)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductExtras.Where(e => e.IsActive))
                // 👇 هذا هو السطر السحري الذي كان مفقوداً، وهو الذي يجلب التقييمات المعتمدة من الداتابيز
                .Include(p => p.ProductReviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            // تمرير خيارات الطباعة وشرائح الخصم للـ View
            ViewBag.PaperOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 1 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.SizeOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 2 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.SidesOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 3 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.FinishOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 4 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.QuantityTiers = await _context.QuantityTiers.Where(q => q.ProductId == id).OrderBy(q => q.MinQuantity).ToListAsync();

            return View(product);
        }

        // دالة البحث عن المنتجات
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SearchQuery = q;

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && (p.NameAr.Contains(q) || p.DescriptionAr.Contains(q)))
                .ToListAsync();

            return View("Index", products);
        }
    }
}