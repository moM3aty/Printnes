using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Printnes.Areas.Admin.Controllers
{
    // تحديد الـ Area الخاصة بلوحة التحكم
    [Area("Admin")]
    // حماية لوحة التحكم بحيث لا يدخلها إلا من يمتلك صلاحية Admin أو SuperAdmin
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // هنا سيتم تجميع إحصائيات الـ ERP (عدد الطلبات، المبيعات اليومية، إلخ)
            return View();
        }
    }
}