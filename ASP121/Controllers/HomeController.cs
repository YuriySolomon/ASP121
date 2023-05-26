using ASP121.Models;
using ASP121.Models.Home;
using ASP121.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;

namespace ASP121.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHashService _hashService;

        public HomeController(ILogger<HomeController> logger, IHashService hashService)
        {
            _logger = logger;
            _hashService = hashService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Intro()
        {
            return View();
        }

        public IActionResult Basics()
        {
            Models.Home.BasicsModel model = new()
            {
                Moment = DateTime.Now
            };
            return View(model);
        }
        public IActionResult Razor()
        {
            Models.Home.RazorModel model = new()
            {
                IntValue = 10,
                BoolValue = true,
                ListValue = new () { "String 1", "String 2", "String 3", "String 4" }
            };
            return View(model);
        }
        public IActionResult Product()
        {           
            var prod = new List<Product>
            {
                new Product
                {
                    Name = "MikroTik cAP",
                    Price = 1684,
                    Image = "/img/AP-1.jpg",
                    Description = "• 2.4 ГГц Wi-Fi\r\n• 300 Мбит/с\r\n• 158 мВт\r\n• 1×100 Мбит LAN\r\n• 2×2 дБи антенна\r\n• ТД/Клиент/Мост\r\n"
                },
                new Product
                {
                    Name = "MikroTik LHG 5",
                    Price = 2423,
                    Image = "/img/AP-2.jpg",
                    Description = "• 5 ГГц Wi-Fi\r\n• 300 Мбит/с\r\n• 316 мВт\r\n• 1×100Мбит LAN\r\n• 24.5 дБи антенна\r\n• 7° покрытие\r\n• Клиент/Мост\r\n"
                },
                new Product
                {
                    Name = "MikroTik mANTBox 2 12s",
                    Price = 3737,
                    Image = "/img/AP-3.jpg",
                    Description = "• 2.4 ГГц Wi-Fi\r\n• 300 Мбит/с\r\n• 1 Вт\r\n• 1×1Гбит LAN\r\n• 12 дБи антенна\r\n• 120° покрытие\r\n• ТД/Клиент/Мост\r\n"
                }

            };
            var model = new ProductModel
            {
                Products = prod
            };
            return View(model);
        }
        public ViewResult Services()
        {
            ViewData["hash"] = _hashService.HashString("123");
            ViewData["obj"] = _hashService.GetHashCode();
            ViewData["ctr"] = this.GetHashCode();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}