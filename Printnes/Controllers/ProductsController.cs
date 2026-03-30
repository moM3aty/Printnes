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

        // GET: /Products
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

        // GET: /Products/Details/5
        // صفحة تفاصيل المنتج (مهمة جداً للخيارات والتسعير)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductExtras.Where(e => e.IsActive)) // جلب الإضافات
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (product == null) return NotFound();

            // جلب خيارات المنتج (ورق، مقاس، أوجه، تشطيب) المرتبطة بهذا المنتج والنشطة فقط
            ViewBag.PaperOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 1 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.SizeOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 2 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.SidesOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 3 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.FinishOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 4 && o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();

            // جلب شرائح الخصم لهذا المنتج
            ViewBag.QuantityTiers = await _context.QuantityTiers.Where(q => q.ProductId == id).OrderBy(q => q.MinQuantity).ToListAsync();

            return View(product);
        }

        // GET: /Products/Search?q=استيكر
        // البحث عن المنتجات
        public async Task<IActionResult> Search(string q)
        {
            ViewBag.SearchQuery = q;

            if (string.IsNullOrWhiteSpace(q))
            {
                return View("Index", Enumerable.Empty<Printnes.Models.Product>());
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && (p.NameAr.Contains(q) || p.NameEn.Contains(q) || p.DescriptionAr.Contains(q)))
                .ToListAsync();

            return View("Index", products); // نستخدم نفس شاشة العرض (Index)
        }
    }
}