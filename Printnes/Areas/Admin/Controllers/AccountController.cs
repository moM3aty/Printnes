/* ============================================
 * الملف: Areas/Admin/Controllers/AccountController.cs
 * كنترولر حساب الأدمن - تسجيل الدخول والخروج
 * ============================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Printnes.Models;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني وكلمة المرور مطلوبان.");
                return View();
            }

            // إنشاء حساب الأدمن الافتراضي تلقائياً عند أول دخول
            if (email.ToLower() == "admin@printnes.co")
            {
                var adminUser = await _userManager.FindByEmailAsync(email);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = "مدير النظام",
                        IsActive = true
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, password);

                    if (createResult.Succeeded)
                    {
                        if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
                        }

                        if (!await _roleManager.RoleExistsAsync("Admin"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Admin"));
                        }

                        if (!await _roleManager.RoleExistsAsync("Accountant"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Accountant"));
                        }

                        if (!await _roleManager.RoleExistsAsync("Customer"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Customer"));
                        }

                        await _userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    }
                }
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // التحقق من أن المستخدم يمتلك صلاحية أدمن
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    bool isAdmin = roles.Any(r => r == "SuperAdmin" || r == "Admin");

                    if (!isAdmin)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "ليس لديك صلاحية الوصول لوحة التحكم. هذه الصفحة للمسؤولين فقط.");
                        return View();
                    }

                    if (!user.IsActive)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "تم تعطيل حسابك. تواصل مع المدير العام.");
                        return View();
                    }
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);
                else
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "تم قفل حسابك مؤقتاً بسبب محاولات الدخول الخاطئة المتكررة.");
            }
            else if (result.RequiresTwoFactor)
            {
                ModelState.AddModelError(string.Empty, "يتطلب التحقق بخطوتين (لم يتم تفعيله بعد).");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة. تأكد من البريد الإلكتروني وكلمة المرور.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}