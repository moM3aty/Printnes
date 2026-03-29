using Printnes.Models;
using System.Collections.Generic;

namespace Printnes.ViewModels
{
    // هذا الكلاس يستخدم لتمرير الإحصائيات المجمعة إلى الصفحة الرئيسية للوحة التحكم (Dashboard Index)
    public class DashboardViewModel
    {
        // الكروت الإحصائية العلوية
        public int NewOrdersCount { get; set; } // الطلبات الجديدة
        public decimal TodaySales { get; set; } // مبيعات اليوم
        public int PrintingOrdersCount { get; set; } // الطلبات قيد الطباعة
        public int TotalCustomersCount { get; set; } // إجمالي العملاء

        // قائمة أحدث الطلبات لعرضها في الجدول السفلي
        public List<Order> RecentOrders { get; set; }
    }
}