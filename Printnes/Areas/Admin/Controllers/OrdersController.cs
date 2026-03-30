/* ============================================
 * الملف: Areas/Admin/Controllers/OrdersController.cs
 * كنترولر إدارة الطلبات مع الفلترة والواتساب
 * ============================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string searchQuery = "",
            byte? statusFilter = null,
            byte? paymentStatusFilter = null,
            string dateFrom = "",
            string dateTo = "",
            string sortBy = "newest")
        {
            var query = _context.Orders.Include(o => o.OrderItems).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(o =>
                    o.OrderNumber.Contains(searchQuery) ||
                    o.CustomerName.Contains(searchQuery) ||
                    o.CustomerPhone.Contains(searchQuery));
            }

            if (statusFilter.HasValue)
                query = query.Where(o => o.OrderStatus == statusFilter.Value);

            if (paymentStatusFilter.HasValue)
                query = query.Where(o => o.PaymentStatus == paymentStatusFilter.Value);

            if (DateTime.TryParse(dateFrom, out var fromDate))
                query = query.Where(o => o.CreatedAt >= fromDate);

            if (DateTime.TryParse(dateTo, out var toDate))
            {
                toDate = toDate.AddDays(1);
                query = query.Where(o => o.CreatedAt < toDate);
            }

            query = sortBy switch
            {
                "oldest" => query.OrderBy(o => o.CreatedAt),
                "total_high" => query.OrderByDescending(o => o.TotalAmount),
                "total_low" => query.OrderBy(o => o.TotalAmount),
                _ => query.OrderByDescending(o => o.CreatedAt)
            };

            var orders = await query.ToListAsync();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.PaymentStatusFilter = paymentStatusFilter;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalOrders = orders.Count;
            ViewBag.TotalRevenue = orders.Where(o => o.PaymentStatus == 1).Sum(o => o.TotalAmount);
            ViewBag.PendingOrders = orders.Count(o => o.OrderStatus == 1);
            ViewBag.PrintingOrders = orders.Count(o => o.OrderStatus == 3);

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.DesignFile)
                .Include(o => o.OrderStatusHistories)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewBag.WhatsappNumber = "+966554804857";
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, byte newStatus, string note, bool sendWhatsApp = false)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var oldStatus = order.OrderStatus;
            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            var history = new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Note = note,
                ChangedBy = User.Identity?.Name ?? "Admin"
            };

            _context.OrderStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            if (sendWhatsApp)
            {
                await SendWhatsAppNotification(order, newStatus, note);
                TempData["SuccessMessage"] = "تم تحديث الحالة وإرسال إشعار واتساب.";
            }
            else
            {
                TempData["SuccessMessage"] = "تم تحديث حالة الطلب بنجاح.";
            }

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToWhatsApp(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            await SendWhatsAppNotification(order, order.OrderStatus, "إعادة إرسال تفاصيل الطلب");
            TempData["SuccessMessage"] = "تم إرسال تفاصيل الطلب عبر الواتساب.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private string BuildWhatsAppMessage(Order order, byte? newStatus = null, string note = "")
        {
            string statusText = newStatus switch
            {
                1 => "📋 جديد - بانتظار المراجعة",
                2 => "🔍 قيد المراجعة والتصميم",
                3 => "🖨️ جاري الطباعة والتجهيز",
                4 => "🚚 تم الشحن والتوصيل",
                5 => "✅ مكتمل",
                _ => GetStatusText(order.OrderStatus)
            };

            string msg = $"🛒 *تفاصيل الطلب #{order.OrderNumber}*\n━━━━━━━━━━━━━━━━━━\n\n";
            msg += $"👤 *العميل:* {order.CustomerName}\n";
            msg += $"📱 *الجوال:* {order.CustomerPhone}\n";
            msg += $"📍 *العنوان:* {order.City} - {order.District} - {order.Address}\n\n";

            if (newStatus.HasValue)
                msg += $"🔔 *تحديث الحالة:* {statusText}\n";

            if (!string.IsNullOrEmpty(note))
                msg += $"💬 *ملاحظة:* {note}\n";

            msg += $"\n📦 *عناصر الطلب:*\n━━━━━━━━━━━━━━━━━━\n";

            if (order.OrderItems != null)
            {
                int i = 1;
                foreach (var item in order.OrderItems)
                {
                    msg += $"{i}. *{item.ProductName}*\n";
                    msg += $"   الكمية: {item.Quantity} | السعر: {item.ItemTotal:F2} ر.س\n";
                    if (!string.IsNullOrEmpty(item.SelectedOptionsJson))
                        msg += $"   الخيارات: {item.SelectedOptionsJson}\n";
                    msg += "\n";
                    i++;
                }
            }

            msg += $"━━━━━━━━━━━━━━━━━━\n";
            msg += $"💰 *المجموع:* {order.SubTotal:F2} ر.س\n";
            msg += $"🚚 *الشحن:* {order.ShippingCost:F2} ر.س\n";
            msg += $"📊 *الضريبة:* {order.TaxAmount:F2} ر.س\n";
            msg += $"💵 *الإجمالي:* *{order.TotalAmount:F2} ر.س*\n";
            msg += $"━━━━━━━━━━━━━━━━━━\n";
            msg += $"_شكراً لاختياركم برنتس 🖨️_";

            return msg;
        }

        private async Task SendWhatsAppNotification(Order order, byte newStatus, string note)
        {
            var message = BuildWhatsAppMessage(order, newStatus, note);
            string cleanPhone = order.CustomerPhone.Replace(" ", "").Replace("-", "").Replace("+", "");
            if (cleanPhone.StartsWith("0")) cleanPhone = "966" + cleanPhone.Substring(1);
            if (!cleanPhone.StartsWith("966")) cleanPhone = "966" + cleanPhone;

            TempData["WhatsAppUrl"] = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(message)}";
        }

        private string GetStatusText(byte status) => status switch
        {
            1 => "جديد",
            2 => "قيد المراجعة",
            3 => "جاري الطباعة",
            4 => "تم الشحن",
            5 => "مكتمل",
            _ => "غير معروف"
        };

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderStatusHistories)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                _context.OrderStatusHistories.RemoveRange(order.OrderStatusHistories);
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الطلب.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}