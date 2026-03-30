/* ============================================
 * الملف: Controllers/HomeController.cs
 * كنترولب الصفحة الرئيسية - بيانات ديناميكية من قاعدة البيانات
 * جلب الأقسام والمنتجات والبانرات
 * ============================================ */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /
        // الصفحة الرئيسية مع بيانات ديناميكية بالكامل
        public async Task<IActionResult> Index()
        {
            // جلب الأقسام النشطة الرئيسية فقط (التي لا تمتلك قسم أب)
            var mainCategories = await _context.Categories
                .Where(c => c.IsActive && string.IsNullOrEmpty(c.ParentMenu))
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            // جلب الأقسام الفرعية مجمعة مع آبائها
            var allCategories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            // جلب أحدث المنتجات المميزة
            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            // جلب أحدث البانرات النشطة
            var activeBanners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();

            // جلب التقييمات المميزة للعرض في الصفحة الرئيسية
            var featuredReviews = await _context.ProductReviews
                .Include(r => r.Product)
                .Where(r => r.IsApproved && r.IsFeatured)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .ToListAsync();

            // جلب إعدادات المتجر (يمكن استبدالها بجدول Settings لاحقاً)
            // var storeSettings = await _context.StoreSettings.FirstOrDefaultAsync();

            ViewBag.MainCategories = mainCategories;
            ViewBag.AllCategories = allCategories;
            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.ActiveBanners = activeBanners;
            ViewBag.FeaturedReviews = featuredReviews;

            return View(featuredProducts);
        }

        // GET: /About
        [Route("About")]
        public IActionResult About()
        {
            return View();
        }

        // GET: /Contact
        [Route("Contact")]
        public IActionResult Contact()
        {
            return View();
        }

        // GET: /Products/Search?q=استيكر
        // البحث في المنتجات بالاسم والوصف
        [Route("Products/Search")]
        public async Task<IActionResult> Search(string q)
        {
            ViewBag.SearchQuery = q;

            if (string.IsNullOrWhiteSpace(q))
            {
                return View("Index", Enumerable.Empty<Product>());
            }

            var products = await _context.Products
               .Include(p => p.Category)
               .Where(p => p.IsActive &&
                   (p.NameAr.Contains(q) ||
                    p.NameEn.ToLower().Contains(q.ToLower()) ||
                    (p.DescriptionAr != null && p.DescriptionAr.Contains(q))))
               .OrderByDescending(p => p.Id)
               .ToListAsync();

            return View("Index", products);
        }

        // GET: /Api/GetQuickStats
        // API سريع لإحصائيات الهيدر
        [Route("Api/GetQuickStats")]
        public async Task<IActionResult> GetQuickStats()
        {
            var productsCount = await _context.Products.CountAsync(p => p.IsActive);
            var categoriesCount = await _context.Categories.CountAsync(c => c.IsActive);
            var ordersCount = await _context.Orders.CountAsync();
            var reviewsCount = await _context.ProductReviews.CountAsync(r => r.IsApproved);

            return Json(new
            {
                productsCount,
                categoriesCount,
                ordersCount,
                reviewsCount
            });
        }
    }
}