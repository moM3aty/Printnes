using Microsoft.AspNetCore.Mvc;

namespace Printnes.Controllers
{
    public class CartController : Controller
    {
        // GET: /Cart
        // واجهة عرض سلة المشتريات
        public IActionResult Index()
        {
            // ملاحظة: في المستقبل سنقوم بربط هذه الصفحة لجلب البيانات من الـ Database أو Session
            // ولكن حالياً ستعتمد على واجهة الـ JavaScript (LocalStorage) التي تعمل بشكل ممتاز لغرض التصميم
            return View();
        }

        // POST: /Cart/Checkout
        // وظيفة إرسال الطلب وحفظه في الداتا بيز (سيتم برمجتها بشكل كامل لاحقاً)
        [HttpPost]
        public IActionResult Checkout()
        {
            // هنا سيتم استقبال الـ JSON الخاص بالسلة من الـ Frontend
            // ثم نقوم بإنشاء Order و OrderItems في قاعدة البيانات

            // حالياً نقوم بإرجاع رسالة نجاح بسيطة أو إعادة توجيه للصفحة الرئيسية
            return RedirectToAction("Index", "Home");
        }
    }
}