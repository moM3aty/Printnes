/* ============================================
 * الملف: Controllers/CategoryController
 * كنترولر الأقسام - مسار ذكي يلتقط أي قسم
 * يمرر بيانات ديناميكية من قاعدة البيانات
 * ============================================ */

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
    public class CategoryController : Controller
    {
        // مسار ذكي يلتقط أي قسم
        [Route("Category/{slug}")]
        public IActionResult Index(string slug)
        {
            // تمرير الـ Slug للـ View
            ViewBag.CategorySlug = slug.ToLower();
            ViewBag.CategoryName = GetCategoryName(slug);
            return View("CategoryTemplate");
        }

        // دالة ترجمة الـ Slug إلى اسم عربي
        private string GetCategoryName(string slug)
        {
            return slug.ToLower() switch
            {
                "stickers" => "طباعة الملصقات والاستيكرات",
                "stickers-rect" => "استيكرات مستطيلة",
                "stickers-circle" => "استيكرات دائرية",
                "stickers-custom" => "استيكرات مخصصة",
                "paperprints" => "مطبوعات ورقية",
                "fileprints" => "طباعة ملفات",
                "business-cards" => "كروت شخصية",
                "flyers" => "فلايرات",
                "brochures" => "بروشورات",
                "letterheads" => "رؤوس خطابات",
                "envelopes" => "ظروف وفنادينات",
                "boxes" => "الطباعة على البوكسات",
                "boxes-food" => "بوكسات أغذية",
                "boxes-cosmetic" => "بوكسات تجميل وأدوات",
                "giftcards" => "بطاقات الشكر والاهداء",
                "giftcards-stickers" => "بطاقات استيكرات",
                "packaging" => "التغليف والتعبئة",
                "bags" => "الطباعة على الأكياس",
                "bags-paper" => "أكياس ورقية",
                "bags-non-woven" => "أكياس قماش",
                "designservices" => "خدمات التصميم",
                "labels" => "ملصقات وفواصل",
                "posters" => "بوسترات",
                "books" => "كتب ومجلات",
                "calendars" => "تقويمات",
                "pens" => "أقلام",
                "stationery" => "قرطاسية",
                "certificates" => "شهادات",
                "id-cards" => "بطاقات تعريف",
                "stickers-removable" => "استيكرات قابلة للإزالة",
                "giftcards-box" => "بطاقات هدايا في بوكس",
                "giftcards-flat" => "بطاقات مسطحة",
                "packaging-boxes" => "تغليف بوكسات",
                "packaging-bags" => "تغليف أكياس",
                "packaging-labels" => "ملصقات تغليف",
                _ => "قسم المنتجات"
            };
        }
    }
}