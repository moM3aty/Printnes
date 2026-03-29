using Microsoft.AspNetCore.Mvc;

namespace Printnes.Controllers
{
    public class BlogController : Controller
    {
        // GET: /Blog
        // صفحة عرض كل المقالات
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Blog/Details/1
        // صفحة عرض تفاصيل المقال (محتوى المقال)
        public IActionResult Details(int? id)
        {
            // لاحقاً سيتم جلب بيانات المقال من قاعدة البيانات باستخدام الـ id
            return View();
        }
    }
}