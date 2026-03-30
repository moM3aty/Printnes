/*
 * الملف: Areas/Admin/Controllers/ReviewsController.cs
 * تم حل مشكلة (Sequence contains no elements) بالتحقق من وجود تقييمات قبل حساب المتوسط
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string searchQuery = "",
            int? productIdFilter = null,
            byte? ratingFilter = null,
            byte? approvalFilter = null,
            string sortBy = "newest")
        {
            var query = _context.ProductReviews.Include(r => r.Product).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(r =>
                    r.ReviewerName.Contains(searchQuery) ||
                    r.ReviewText.Contains(searchQuery) ||
                    (r.Product != null && r.Product.NameAr.Contains(searchQuery)));
            }

            if (productIdFilter.HasValue && productIdFilter.Value > 0)
                query = query.Where(r => r.ProductId == productIdFilter.Value);

            if (ratingFilter.HasValue && ratingFilter.Value > 0)
                query = query.Where(r => r.Rating == ratingFilter.Value);

            if (approvalFilter.HasValue)
                query = query.Where(r => r.IsApproved == (approvalFilter.Value == 1));

            query = sortBy switch
            {
                "oldest" => query.OrderBy(r => r.CreatedAt),
                "rating_high" => query.OrderByDescending(r => r.Rating),
                "rating_low" => query.OrderBy(r => r.Rating),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var reviews = await query.ToListAsync();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.ProductIdFilter = productIdFilter;
            ViewBag.RatingFilter = ratingFilter;
            ViewBag.ApprovalFilter = approvalFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.Products = new SelectList(
                await _context.Products.Where(p => p.IsActive).OrderBy(p => p.NameAr).ToListAsync(),
                "Id", "NameAr");

            // --- حل المشكلة هنا ---
            ViewBag.TotalReviews = await _context.ProductReviews.CountAsync();
            ViewBag.PendingApproval = await _context.ProductReviews.CountAsync(r => !r.IsApproved);

            // التحقق من وجود عناصر قبل استدعاء AverageAsync
            bool hasReviews = await _context.ProductReviews.AnyAsync();
            ViewBag.AverageRating = hasReviews ? await _context.ProductReviews.AverageAsync(r => (double)r.Rating) : 0.0;

            ViewBag.TotalApproved = await _context.ProductReviews.CountAsync(r => r.IsApproved);

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تمت الموافقة على التقييم.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إخفاء التقييم.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.IsFeatured = !review.IsFeatured;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = review.IsFeatured ? "تم تعيين التقييم كمميز." : "تم إزالة التمييز.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string adminReply)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.AdminReply = adminReply;
                review.AdminReplyDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حفظ الرد.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف التقييم نهائياً.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAllPending()
        {
            var pending = await _context.ProductReviews.Where(r => !r.IsApproved).ToListAsync();
            foreach (var review in pending)
            {
                review.IsApproved = true;
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"تمت الموافقة على {pending.Count} تقييم.";
            return RedirectToAction(nameof(Index));
        }
    }
}