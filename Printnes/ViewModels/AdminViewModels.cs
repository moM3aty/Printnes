// الملف: ViewModels/AdminViewModels.cs

using Printnes.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Printnes.ViewModels
{
    // كلاس لتمرير الإحصائيات المجمعة إلى الصفحة الرئيسية للوحة التحكم (Dashboard Index)
    public class DashboardViewModel
    {
        public int TodayOrdersCount { get; set; }
        public decimal TodaySales { get; set; }
        public int MonthOrdersCount { get; set; }
        public decimal MonthSales { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCustomers { get; set; }

        public int PendingOrders { get; set; }
        public int ReviewingOrders { get; set; }
        public int PrintingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }

        public double OrdersGrowthPercent { get; set; }
        public double RevenueGrowthPercent { get; set; }

        public List<Order> RecentOrders { get; set; }
        public List<TopProductDto> TopProducts { get; set; }

        public int PendingReviews { get; set; }
        public int TotalReviews { get; set; }
    }

    public class TopProductDto
    {
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // كلاس لصفحة إعدادات النظام والمتجر
    public class StoreSettingsViewModel
    {
        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [StringLength(100)]
        public string StoreName { get; set; }
        public string PrivacyPolicy { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankIban { get; set; }
        public string FAQ { get; set; }
        public string RefundPolicy { get; set; }
        public string TermsAndConditions { get; set; }
        public string TaxNumber { get; set; }
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        public string ContactPhone { get; set; }
        public string StoreLogoUrl { get; set; } = "https://cdn.salla.sa/rDvVQ/cOd3EfWfrDP0n64aE7wrEEXzWprxjvx6kvvsO0aR.png";

        public string WhatsappNumber { get; set; }
        public string StoreDescription { get; set; }
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TiktokUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string Currency { get; set; }

        public bool EnableReviews { get; set; }
        public bool EnableUserRegistration { get; set; }
        public bool EnableGuestCheckout { get; set; }

        public decimal MinimumOrderAmount { get; set; }
        public decimal FreeShippingAbove { get; set; }

        [Required(ErrorMessage = "نسبة الضريبة مطلوبة")]
        [Range(0, 100, ErrorMessage = "النسبة يجب أن تكون بين 0 و 100")]
        public decimal TaxPercentage { get; set; }

        [Required(ErrorMessage = "تكلفة الشحن مطلوبة")]
        [Range(0, 1000, ErrorMessage = "قيمة غير منطقية")]
        public decimal DefaultShippingCost { get; set; }
    }

    // كلاسات لإدارة الصلاحيات والمستخدمين
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // قائمة بجميع الصلاحيات المتوفرة
        public List<RoleSelection> Roles { get; set; }
    }

    public class RoleSelection
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class UserDetailsViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; }
        public int AccessFailedCount { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime? LastLoginDate { get; set; }
    }
}