using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            // 1. استخراج القيم في متغيرات أولاً لتجنب مشاكل الـ await داخل الـ Object Initializer
            var todayOrdersCount = await _context.Orders.CountAsync(o => o.CreatedAt.Date == today);

            var todaySales = await _context.Orders
                .Where(o => o.CreatedAt.Date == today && o.PaymentStatus == 1)
                .SumAsync(o => o.TotalAmount);

            var monthOrdersCount = await _context.Orders.CountAsync(o => o.CreatedAt >= thisMonthStart);

            var monthSales = await _context.Orders
                .Where(o => o.CreatedAt >= thisMonthStart && o.PaymentStatus == 1)
                .SumAsync(o => o.TotalAmount);

            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.Where(o => o.PaymentStatus == 1).SumAsync(o => o.TotalAmount);
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalCategories = await _context.Categories.CountAsync(c => c.IsActive);
            var totalCustomers = await _context.Users.CountAsync();

            var pendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == 1);
            var reviewingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == 2);
            var printingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == 3);
            var shippingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == 4);
            var completedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == 5);

            // حساب النمو
            var lastMonthOrdersCount = await _context.Orders.CountAsync(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < thisMonthStart);
            var ordersGrowthPercent = await CalculateGrowth(monthOrdersCount, lastMonthOrdersCount);

            var lastMonthSales = await _context.Orders.Where(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < thisMonthStart && o.PaymentStatus == 1).SumAsync(o => o.TotalAmount);
            var revenueGrowthPercent = await CalculateGrowth(monthSales, lastMonthSales);

            // 2. إصلاح خطأ ToList واستخدام ToListAsync بدلاً منها
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            var topProducts = await _context.OrderItems
                .GroupBy(oi => oi.ProductName)
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.ItemTotal)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(5)
                .ToListAsync();

            var pendingReviews = await _context.ProductReviews.CountAsync(r => !r.IsApproved);
            var totalReviews = await _context.ProductReviews.CountAsync(r => r.IsApproved);

            // 3. تمرير المتغيرات الجاهزة إلى الـ ViewModel
            var viewModel = new DashboardViewModel
            {
                TodayOrdersCount = todayOrdersCount,
                TodaySales = todaySales,
                MonthOrdersCount = monthOrdersCount,
                MonthSales = monthSales,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalCustomers = totalCustomers,
                PendingOrders = pendingOrders,
                ReviewingOrders = reviewingOrders,
                PrintingOrders = printingOrders,
                ShippingOrders = shippingOrders,
                CompletedOrders = completedOrders,
                OrdersGrowthPercent = ordersGrowthPercent,
                RevenueGrowthPercent = revenueGrowthPercent,
                RecentOrders = recentOrders,
                TopProducts = topProducts,
                PendingReviews = pendingReviews,
                TotalReviews = totalReviews
            };

            return View(viewModel);
        }

        private async Task<double> CalculateGrowth(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round((double)(((current - previous) / previous) * 100), 1);
        }
    }
}