/* ============================================
 * الملف: Controllers/AccountController.cs
 * كنترولر حساب العميل - تسجيل الدخول والتسجيل والملف الشخصي
 * الصلاحيات: Customer
 * ============================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Printnes.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Printnes.Controllers
{
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
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["LoginError"] = "البريد الإلكتروني وكلمة المرور مطلوبان.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && !user.IsActive)
                {
                    await _signInManager.SignOutAsync();
                    TempData["LoginError"] = "تم تعطيل حسابك. تواصل مع الدعم الفني.";
                    return View();
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                TempData["LoginError"] = "تم قفل حسابك مؤقتاً بسبب محاولات الدخول الخاطئة.";
            else
                TempData["LoginError"] = "بيانات الدخول غير صحيحة.";

            return View();
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string phone, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                TempData["RegisterError"] = "كلمة المرور وتأكيدها غير متطابقتين.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["RegisterError"] = "جميع الحقول المطلوبة يجب تعبئتها.";
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["RegisterError"] = "هذا البريد الإلكتروني مسجل مسبقاً. جرب تسجيل الدخول أو استخدم بريد آخر.";
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = phone,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, password);

            if (createResult.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Customer"))
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));

                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, true);

                TempData["SuccessMessage"] = "تم إنشاء حسابك بنجاح! مرحباً بك في برنتس.";
                return RedirectToAction("Index", "Home");
            }

            TempData["RegisterError"] = string.Join("<br>", createResult.Errors.Select(e => e.Description));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string phone)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!string.IsNullOrWhiteSpace(fullName))
                user.FullName = fullName;

            if (!string.IsNullOrWhiteSpace(phone))
                user.PhoneNumber = phone;

            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = "تم تحديث بياناتك بنجاح.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (newPassword != confirmNewPassword)
            {
                TempData["ProfileError"] = "كلمة المرور الجديدة وتأكيدها غير متطابقتين.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح.";
            }
            else
            {
                TempData["ProfileError"] = string.Join("<br>", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}