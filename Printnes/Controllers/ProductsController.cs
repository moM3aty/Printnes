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
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (product == null) return NotFound();

            // إرسال خيارات المنتج الديناميكية من الداتابيز للـ View
            ViewBag.PaperOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 1 && o.IsActive).ToListAsync();
            ViewBag.SizeOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 2 && o.IsActive).ToListAsync();
            ViewBag.SidesOptions = await _context.ProductOptions.Where(o => o.ProductId == id && o.OptionType == 3 && o.IsActive).ToListAsync();

            return View(product);
        }
    }
}