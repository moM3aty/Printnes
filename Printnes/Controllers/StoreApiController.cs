using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: api/StoreApi/GetProductsByCategory?slug=stickers
        [HttpGet("GetProductsByCategory")]
        public async Task<IActionResult> GetProductsByCategory([FromQuery] string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest("يجب توفير رابط القسم (Slug)");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category.Slug.ToLower() == slug.ToLower() && p.IsActive)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.NameAr,
                    price = 0, // سيتم استبدالها لاحقاً بـ: p.PricingMatrices.Min(m => m.UnitPrice)
                    badge = "منتج مميز",
                    img = p.CoverImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/StoreApi/GetFeaturedProducts
        [HttpGet("GetFeaturedProducts")]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id) // مثال: جلب أحدث المنتجات
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
    }
}