using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")] // الصلاحيات المطلوبة
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض جميع الطلبات
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        // عرض تفاصيل طلب محدد
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // تحديث حالة الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, byte newStatus, string note)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var oldStatus = order.OrderStatus;
            order.OrderStatus = newStatus;
            order.UpdatedAt = System.DateTime.UtcNow;

            // تسجيل الحركة في سجل الطلبات
            var history = new Models.OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Note = note,
                ChangedBy = User.Identity.Name // اسم الموظف الذي قام بالتعديل
            };

            _context.OrderStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
    }
}