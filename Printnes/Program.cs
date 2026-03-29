using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Printnes.Data;
using Printnes.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. إعداد قاعدة البيانات (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. إعداد نظام الهوية والصلاحيات (Identity & Roles)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. إعداد مسارات الدخول وملفات تعريف الارتباط (Cookies)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login"; // توجيه الموظفين لصفحة الدخول
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// 4. إضافة المتحكمات والواجهات
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. إعداد الـ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// تفعيل التحقق من الهوية والصلاحيات
app.UseAuthentication();
app.UseAuthorization();

// 6. إعداد الـ Routing (توجيه الروابط)
// مسار لوحة التحكم (ERP)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// المسار الافتراضي (واجهة العميل)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();