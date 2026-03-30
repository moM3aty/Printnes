/* ============================================
 * الملف: Controllers/PagesController
 * كنترولر الصفحات الثابتة (Policy, FAQ, Privacy, Terms, Complaints, Rewards)
 * صفحات سياسة الخصوصية والأسئلة شائعة وسياسة الطباعة
 * ============================================ */

using Microsoft.AspNetCore.Mvc;

namespace Printnes.Controllers
{
    public class PagesController : Controller
    {
        // GET: /Privacy - سياسة الخصوصية
        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /FAQ - الأسئلة الشائعة
        [Route("FAQ")]
        public IActionResult Faq()
        {
            return View();
        }

        // GET: /RefundPolicy - سياسة الاستبدال والاسترجاع
        [Route("RefundPolicy")]
        public IActionResult RefundPolicy()
        {
            return View();
        }

        // GET: /Terms - سياسة الطباعة والتصميم
        [Route("Terms")]
        public IActionResult Terms()
        {
            return View();
        }

        // GET: /Complaints - الشكاوي والاقتراحات
        [Route("Complaints")]
        public IActionResult Complaints()
        {
            return View();
        }

        // GET: /Rewards - نظام المكافآت
        [Route("Rewards")]
        public IActionResult Rewards()
        {
            return View();
        }
    }
}