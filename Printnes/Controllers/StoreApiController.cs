/*
 * الملف: Controllers/Api/StoreApiController.cs
 * تم إضافة دالة UploadDesign لاستقبال صور تصاميم العملاء من ستوديو الـ 3D
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Printnes.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StoreApiController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("GetProductsByCategory")]
        public async Task<IActionResult> GetProductsByCategory([FromQuery] string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return BadRequest("يجب توفير رابط القسم (Slug)");

            var slugLower = slug.ToLower();

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

        // دالة جديدة: رفع ملف تصميم العميل من ستوديو الـ 3D
        [HttpPost("UploadDesign")]
        public async Task<IActionResult> UploadDesign(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "لم يتم إرسال أي ملف." });

            try
            {
                // التحقق من أن المجلد موجود، وإذا لم يكن نقوم بإنشائه
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "designs");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // إنشاء اسم فريد للملف لتجنب التكرار
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // الرابط الذي سيتم حفظه في الطلب
                string fileUrl = $"/uploads/designs/{uniqueFileName}";

                return Ok(new { success = true, url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ أثناء رفع الملف: " + ex.Message });
            }
        }
    }
}