using Microsoft.AspNetCore.Mvc;

namespace Printnes.Controllers
{
    public class CategoryController : Controller
    {


        [Route("Category/Stickers")]
        public IActionResult Stickers() => View();

        [Route("Category/Stickers-Rect")]
        public IActionResult StickersRect() => View("StickersRect"); // يمكن إنشاء View مخصص لها

        [Route("Category/Boxes")]
        public IActionResult Boxes() => View();

        [Route("Category/Packaging")]
        public IActionResult Packaging() => View();

        [Route("Category/Bags")]
        public IActionResult Bags() => View();

        [Route("Category/PaperPrints")]
        public IActionResult PaperPrints() => View();

        [Route("Category/GiftCards")]
        public IActionResult GiftCards() => View();

        [Route("Category/DesignServices")]
        public IActionResult DesignServices() => View();

        [Route("Packages")]
        public IActionResult Packages() => View();
    }
}