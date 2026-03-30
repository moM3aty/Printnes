/* ============================================
 * الملف: Controllers/CartController.cs
 * كنترولب سلة المشتريات - تعتمد على LocalStorage
 * إتمام الطلب عبر الواتساب
 * ============================================ */

using Microsoft.AspNetCore.Mvc;

namespace Printnes.Controllers
{
    public class CartController : Controller
    {
        // GET: /Cart
        // واجهة عرض سلة المشتريات (البيانات من LocalStorage)
        public IActionResult Index()
        {
            // لا نحتاج Model لأننا نعتمد على LocalStorage عبر JavaScript
            return View();
        }

        // GET: /Cart/GetCartCount
        // API للحصول على عدد عناصر السلة (للتحديث بدون إعادة تحميل الصفحة)
        public IActionResult GetCartCount()
        {
            // يتم استدعاء هذا من الـ JS عبر AJAX لو احتجنا لذلك
            return Ok(new { count = 0 });
        }
    }
}