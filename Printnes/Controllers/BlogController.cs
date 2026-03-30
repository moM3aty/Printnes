using Microsoft.AspNetCore.Mvc;

namespace Printnes.Controllers
{
    public class BlogController : Controller
    {
        // GET: /Blog
        // صفحة عرض كل المقالات (المدونة)
        public IActionResult Index()
        {
            // لاحقاً يمكن جلب المقالات من قاعدة البيانات
            return View();
        }

        // GET: /Blog/Details/1
        // صفحة عرض تفاصيل المقال (قراءة المقال)
        public IActionResult Details(int? id)
        {
            // لاحقاً يمكن استخدام الـ id لجلب المقال الفعلي من قاعدة البيانات
            return View();
        }
    }
}