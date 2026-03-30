/*
 * الملف: Controllers/OrderController.cs
 * كنترولر الطلبات - إتمام الطلب عبر الواتساب (تم إصلاح الأخطاء الحسابية والأقواس)
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;
using Printnes.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
        // صفحة إتمام الطلب مع ملخص السلة وبيانات الشحن
        public IActionResult Checkout()
        {
            return View(new CheckoutViewModel());
        }

        // POST: /Order/Checkout
        // استقبال الطلب وحفظه في قاعدة البيانات ثم إرساله للواتساب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.CartItemsJson) || model.CartItemsJson == "[]")
            {
                ModelState.AddModelError("", "السلة فارغة! لا يمكن إتمام الطلب.");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. فك تشفير JSON السلة
                var cartItems = JsonSerializer.Deserialize<List<CartItemDto>>(model.CartItemsJson);

                decimal subTotal = 0;

                // 2. إنشاء الطلب الرئيسي
                var order = new Order
                {
                    OrderNumber = "PN-" + DateTime.Now.ToString("yyMM") + "-" + new Random().Next(10000, 99999),
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    City = model.City,
                    District = model.District,
                    Address = model.Address,
                    Notes = model.Notes,
                    PaymentMethod = model.PaymentMethod,
                    OrderStatus = 1,
                    PaymentStatus = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 3. إضافة عناصر الطلب
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductName = item.Name,
                        Quantity = item.Quantity > 0 ? item.Quantity : 1,
                        CalculatedUnitPrice = item.Price,
                        ItemTotal = item.Price * (item.Quantity > 0 ? item.Quantity : 1),
                        SelectedOptionsJson = item.Details != null ? JsonSerializer.Serialize(item.Details) : null,
                        ExtrasTotal = 0
                    };

                    subTotal += orderItem.ItemTotal;
                    _context.OrderItems.Add(orderItem);
                }

                // 4. حساب المجاميع المالية
                order.SubTotal = subTotal;
                order.ShippingCost = 25m;
                order.TaxAmount = (subTotal + order.ShippingCost) * 0.15m;
                order.TotalAmount = order.SubTotal + order.ShippingCost + order.TaxAmount;

                _context.Orders.Update(order);

                // 5. تسجيل في سجل الحركات
                var history = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    NewStatus = 1,
                    ChangedBy = model.CustomerName,
                    Note = "تم إنشاء الطلب من قبل العميل عبر الموقع"
                };

                _context.OrderStatusHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 6. توجيه لصفحة النجاح مع رقم الطلب
                return RedirectToAction("Success", new { orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ الطلب. " + ex.Message);
                return View(model);
            }
        }

        // GET: /Order/Success?orderNumber=PN-xxxx
        // صفحة تأكيد إتمام الطلب مع رابط الواتساب
        public IActionResult Success(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
                return RedirectToAction("Index", "Home");

            ViewBag.OrderNumber = orderNumber;
            return View();
        }

        // POST: /Order/SendWhatsApp
        // API لإرسال الطلب للواتساب من صفحة إتمام الطلب (بدون حفظ في DB)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendWhatsApp(CheckoutViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CartItemsJson) || model.CartItemsJson == "[]")
            {
                return Json(new { success = false, message = "السلة فارغة" });
            }

            var cartItems = JsonSerializer.Deserialize<List<CartItemDto>>(model.CartItemsJson);

            // بناء رسالة واتساب
            string message = BuildWhatsAppMessage(cartItems, model);

            string cleanPhone = model.CustomerPhone.Replace(" ", "").Replace("-", "").Replace("+", "");
            if (cleanPhone.StartsWith("0"))
                cleanPhone = "966" + cleanPhone.Substring(1);
            if (!cleanPhone.StartsWith("966"))
                cleanPhone = "966" + cleanPhone;

            string waUrl = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(message)}";

            return Json(new { success = true, url = waUrl });
        }

        // بناء رسالة واتساب للطلب
        private string BuildWhatsAppMessage(List<CartItemDto> items, CheckoutViewModel model)
        {
            decimal total = items.Sum(i => i.Price * (i.Quantity > 0 ? i.Quantity : 1));

            string msg = $"🛒 *طلب جديد من متجر برنتس*\n";
            msg += $"━━━━━━━━━━━━━━━━━━\n\n";
            msg += $"👤 *العميل:* {model.CustomerName}\n";
            msg += $"📱 *الجوال:* {model.CustomerPhone}\n";
            msg += $"📍 *العنوان:* {model.City} - {model.District} - {model.Address}\n\n";

            msg += $"📦 *عناصر الطلب:*\n";
            msg += $"━━━━━━━━━━━━━━━━━━\n";

            int idx = 1;
            foreach (var item in items)
            {
                msg += $"{idx}. *{item.Name}*\n";
                // تم إضافة القوس الناقص هنا لحل المشكلة
                msg += $"   الكمية: {item.Quantity} | السعر: {(item.Price * (item.Quantity > 0 ? item.Quantity : 1)):F2} ر.س\n";

                if (item.Details != null)
                {
                    if (!string.IsNullOrEmpty(item.Details.Design))
                        msg += $"   التصميم: {item.Details.Design}\n";
                    if (!string.IsNullOrEmpty(item.Details.PaperType))
                        msg += $"   الورق: {item.Details.PaperType}\n";
                    if (!string.IsNullOrEmpty(item.Details.Sides))
                        msg += $"   الأوجه: {item.Details.Sides}\n";
                    if (!string.IsNullOrEmpty(item.Details.Cover))
                        msg += $"   الحماية: {item.Details.Cover}\n";
                    if (!string.IsNullOrEmpty(item.Details.Corners))
                        msg += $"   الأطراف: {item.Details.Corners}\n";
                }

                msg += "\n";
                idx++;
            }

            msg += $"━━━━━━━━━━━━━━━━━━\n";
            msg += $"💰 *الإجمالي التقريبي:* {total:F2} ر.س\n";
            msg += $"🚚 *الشحن (تقريباً):* 25.00 ر.س\n";
            // تم إضافة m بجانب 0.15 لحل الخطأ الخاص بنوع البيانات
            msg += $"📊 *الضريبة (15%):* {(total * 0.15m):F2} ر.س\n";
            // تم إضافة m بجانب الأرقام 25 و 0.15
            msg += $"💵 *الإجمالي النهائي:* *{(total + 25m + (total * 0.15m)):F2} ر.س*\n";
            msg += $"━━━━━━━━━━━━━━━━━━\n";
            msg += $"_شكراً لاختياركم برنتس للطباعة 🖨️_\n";
            msg += $"_تم الإرسال عبر واتساب تلقائياً_{DateTime.Now:HH:mm}_";

            return msg;
        }
    }
}