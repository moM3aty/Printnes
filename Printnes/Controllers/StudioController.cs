using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Printnes.Data;
using Printnes.Models;
using Printnes.Helpers; // إضافة هذا السطر
using System;
using System.IO;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
    public class StudioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudioController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Studio
        public IActionResult Index()
        {
            // قراءة إعدادات الاستوديو المستقلة وتمريرها للـ View
            var studioSettings = StudioSettingsManager.LoadSettings();
            ViewBag.StudioSettings = studioSettings;

            return View();
        }

        // POST: /Studio/SaveDesign
        [HttpPost]
        public async Task<IActionResult> SaveDesign([FromBody] DesignPayload payload)
        {
            try
            {
                string imageUrl = "";

                // 1. تحويل صورة الـ Base64 وحفظها في السيرفر (مع التأكد من التنسيق)
                if (!string.IsNullOrEmpty(payload.PreviewImageBase64) && payload.PreviewImageBase64.Contains(","))
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "designs");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // استخراج الداتا الصافية من الـ Base64
                    var base64Data = payload.PreviewImageBase64.Split(',')[1];
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    string fileName = Guid.NewGuid().ToString() + ".png";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    imageUrl = "/uploads/designs/" + fileName;
                }

                // 2. حفظ البيانات في الداتا بيز
                var newDesign = new CustomDesign
                {
                    ProductType = payload.ProductType,
                    Length = payload.Length,
                    Width = payload.Width,
                    Height = payload.Height,
                    ColorHex = payload.ColorHex,
                    CanvasJsonData = payload.CanvasJsonData, 
                    PreviewImageUrl = imageUrl,
                    EstimatedPrice = payload.Price,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CustomDesigns.Add(newDesign);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, designId = newDesign.Id, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDesignData(int id)
        {
            var design = await _context.CustomDesigns.FindAsync(id);
            if (design == null)
            {
                return NotFound(new { success = false, message = "التصميم غير موجود" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = design.Id,
                    productType = design.ProductType,
                    length = design.Length,
                    width = design.Width,
                    height = design.Height,
                    colorHex = design.ColorHex,
                    canvasJsonData = design.CanvasJsonData,
                    previewImageUrl = design.PreviewImageUrl
                }
            });
        }

        public class DesignPayload
        {
            public string ProductType { get; set; }
            public decimal Length { get; set; }
            public decimal Width { get; set; }
            public decimal Height { get; set; }
            public string ColorHex { get; set; }
            public string CanvasJsonData { get; set; }
            public string PreviewImageBase64 { get; set; }
            public decimal Price { get; set; }
        }
    }
}