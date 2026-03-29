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

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // GET: /Admin/Account/Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            // إذا كان المستخدم مسجل دخول بالفعل، وجهه للوحة التحكم
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Admin/Account/Login
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

            // محاولة تسجيل الدخول
            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);
                else
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة.");
            return View();
        }

        // POST: /Admin/Account/Logout
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