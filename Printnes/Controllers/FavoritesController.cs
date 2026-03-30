/*
 * الملف: Controllers/FavoritesController.cs
 * كنترولر إدارة المفضلات للمستخدمين المسجلين
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Favorites
        // عرض صفحة المفضلات
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var favorites = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                    .ThenInclude(p => p.Category)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return View(favorites);
        }

        // POST: /Favorites/Toggle/5
        // إضافة أو إزالة من المفضلات (يعمل كـ Toggle)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً", isLoggedIn = false });
            }

            var existing = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            bool isNowFavorite;

            if (existing != null)
            {
                // إزالة من المفضلات
                _context.UserFavorites.Remove(existing);
                isNowFavorite = false;
            }
            else
            {
                // إضافة للمفضلات
                var favorite = new UserFavorite
                {
                    UserId = userId,
                    ProductId = productId
                };
                _context.UserFavorites.Add(favorite);
                isNowFavorite = true;
            }

            await _context.SaveChangesAsync();

            // عدد المفضلات الكلي للمستخدم
            int totalFavorites = await _context.UserFavorites.CountAsync(f => f.UserId == userId);

            return Json(new { success = true, isFavorite = isNowFavorite, total = totalFavorites });
        }

        // GET: /Favorites/Check/5
        // التحقق مما إذا كان المنتج في المفضلات (لتحديث أيقونة القلب في الصفحة)
        [HttpGet]
        public async Task<IActionResult> Check(int productId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { isFavorite = false });
            }

            var isFavorite = await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId);

            return Json(new { isFavorite });
        }

        // GET: /Favorites/Count
        // عدد المفضلات (لعرضه في الهيدر)
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var userId = _userManager.GetUserId(User);
            int count = 0;

            if (!string.IsNullOrEmpty(userId))
            {
                count = await _context.UserFavorites.CountAsync(f => f.UserId == userId);
            }

            return Json(new { count });
        }

        // POST: /Favorites/Remove/5
        // حذف من المفضلات (من صفحة المفضلات مباشرة)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = _userManager.GetUserId(User);

            var favorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (favorite != null)
            {
                _context.UserFavorites.Remove(favorite);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "تم إزالة المنتج من المفضلات.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Favorites/GetUserFavorites
        // جلب قائمة معرفات المنتجات المفضلة (للاستخدام في الـ JavaScript)
        [HttpGet]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { productIds = new int[0] });
            }

            var productIds = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.ProductId)
                .ToListAsync();

            return Json(new { productIds });
        }
    }
}