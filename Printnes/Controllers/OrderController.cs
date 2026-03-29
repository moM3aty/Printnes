using Microsoft.AspNetCore.Mvc;
using Printnes.Data;
using Printnes.Models;
using Printnes.ViewModels;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Order/Checkout
        public IActionResult Checkout()
        {
            return View(new CheckoutViewModel());
        }

        // POST: /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 1. إنشاء الطلب الأساسي
                var order = new Order
                {
                    OrderNumber = "PN-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999),
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    City = model.City,
                    District = model.District,
                    Address = model.Address,
                    PostalCode = model.PostalCode,
                    Notes = model.Notes,
                    PaymentMethod = model.PaymentMethod,
                    OrderStatus = 1, // New
                    PaymentStatus = 0, // Pending
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // يمكنك لاحقاً فك تشفير JSON الخاص بالسلة لحساب المجاميع الفعلية
                // var cartItems = JsonSerializer.Deserialize<List<CartItemDto>>(model.CartItemsJson);
                // order.SubTotal = cartItems.Sum(c => c.Price * c.Quantity);
                // ... إلخ

                order.SubTotal = 0; // سيتم تحديثه بعد إضافة العناصر
                order.TaxAmount = 0;
                order.ShippingCost = 25; // مثال: 25 ريال شحن
                order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingCost;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 2. تسجيل حالة الطلب في الـ History
                var history = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    NewStatus = 1,
                    ChangedBy = "System",
                    Note = "تم إنشاء الطلب من قبل العميل عبر الموقع"
                };
                _context.OrderStatusHistories.Add(history);
                await _context.SaveChangesAsync();

                // توجيه العميل لصفحة النجاح
                return RedirectToAction("Success", new { orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء معالجة الطلب، يرجى المحاولة لاحقاً. " + ex.Message);
                return View(model);
            }
        }

        // GET: /Order/Success
        public IActionResult Success(string orderNumber)
        {
            ViewBag.OrderNumber = orderNumber;
            return View();
        }
    }
}