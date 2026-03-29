using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Printnes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")] // لا يدخلها إلا المدير العام
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: /Admin/Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        // GET: /Admin/Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                // التحقق من عدم وجود الصلاحية مسبقاً
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "هذه الصلاحية موجودة بالفعل.");
                }
            }
            else
            {
                ModelState.AddModelError("", "اسم الصلاحية مطلوب.");
            }

            return View();
        }
    }
}