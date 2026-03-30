/*
 * الملف: Controllers/Api/StoreApiController.cs
 * تم التعديل لحل مشكلة الأقسام الفارغة: الآن يجلب المنتجات من القسم المحدد ومن كافة الأقسام الفرعية التابعة له.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Printnes.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StoreApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetProductsByCategory")]
        public async Task<IActionResult> GetProductsByCategory([FromQuery] string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return BadRequest("يجب توفير رابط القسم (Slug)");

            var slugLower = slug.ToLower();

            // تعديل جوهري: نجلب المنتجات التي يطابق قسمها الـ slug الممرر 
            // أو التي يكون قسمها الأب يطابق الـ slug الممرر (لحل مشكلة الأقسام الرئيسية الفارغة)
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive &&
                            (p.Category.Slug.ToLower() == slugLower ||
                            (p.Category.ParentMenu != null && p.Category.ParentMenu.ToLower() == slugLower)))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.NameAr,
                    price = 0,
                    badge = "منتج",
                    img = p.CoverImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("GetFeaturedProducts")]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.NameAr,
                    price = 0,
                    badge = "الأكثر مبيعاً",
                    img = p.CoverImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("CalculatePrice")]
        public async Task<IActionResult> CalculatePrice(int productId, int paperId, int sizeId, int sidesId, int quantity, [FromQuery] int[] extraIds)
        {
            var matrix = await _context.PricingMatrices
                .FirstOrDefaultAsync(m => m.ProductId == productId && m.PaperOptionId == paperId && m.SizeOptionId == sizeId && m.SidesOptionId == sidesId);

            if (matrix == null)
                return Ok(new { success = false, message = "غير متاح بهذه المواصفات" });

            decimal total = matrix.BasePrice + (matrix.UnitPrice * quantity);

            if (extraIds != null && extraIds.Length > 0)
            {
                var extras = await _context.ProductExtras.Where(e => extraIds.Contains(e.Id)).ToListAsync();
                total += extras.Sum(e => e.Price);
            }

            var tier = await _context.QuantityTiers
                .Where(q => q.ProductId == productId && quantity >= q.MinQuantity && quantity <= q.MaxQuantity)
                .OrderByDescending(q => q.DiscountPercent)
                .FirstOrDefaultAsync();

            decimal discountAmount = 0;
            if (tier != null)
            {
                discountAmount = total * (tier.DiscountPercent / 100m);
                total -= discountAmount;
            }

            return Ok(new
            {
                success = true,
                total = total,
                basePrice = matrix.BasePrice,
                unitPrice = matrix.UnitPrice,
                discount = discountAmount
            });
        }
    }
}