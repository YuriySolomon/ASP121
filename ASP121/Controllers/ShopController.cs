using ASP121.Data;
using ASP121.Models.Shop;
using ASP121.Models.User;
using ASP121.Services.Hash;
using ASP121.Views.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace ASP121.Controllers
{
    public class ShopController : Controller
    {
        private readonly DataContext _dataContext;
        public ShopController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public IActionResult Index()
        {
            ShopIndexViewModel model = new()
            {
                ProductGroups = _dataContext.ProductGroups
                    .Where(g => g.DeleteDt == null).ToList(),
                Products = _dataContext.Products
                    .Include(p => p.Rates)  // заповнює навігаційну вдастивість (включає до запиту SQL)
                    .Where(p => p.DeleteDt == null).ToList()

            };
            if (HttpContext.Session.Keys.Contains("AddMessage"))
            {
                model.AddMessage = HttpContext.Session.GetString("AddMessage");
                HttpContext.Session.Remove("AddMessage");
            }
            return View(model);
        }

        [HttpPost]
        public RedirectToActionResult AddProduct(ShopIndexFormModel model)
        {
            // перевіряємо модел, зберігаємо файл, додаємо до БД. повертаємо повідомлення
            String errorMessage = _ValidateModel(model);
            if (errorMessage != String.Empty)
            {
                // є помилка валідації
                HttpContext.Session.SetString("AddMessage", errorMessage);
            }
            else
            {
                // перевірка успішна
                HttpContext.Session.SetString("AddMessage", "Додано успішно");
            }
            return RedirectToAction(nameof(Index));
        }

        private String _ValidateModel(ShopIndexFormModel? model)
        {
            if (model == null) { return "Дані не передані"; }
            if (String.IsNullOrEmpty(model.Title)) { return "Назва не може бути порожнім"; }
            if (model.Price == 0)
            {
                // можлива помилка декодування через локалізацію (1.5/1,5)
                model.Price = Convert.ToSingle(Request.Form["productPrice"].First()?.Replace(',','.'), CultureInfo.InvariantCulture);
            }
            if (model.Price <= 0)
            {
                return "Товар не може коштувати 0 та менше грн.";
            }
            String? newName = null;
            if (model.Image != null)  // є файл
            {
                // визначаємо тип (розширення) файлу
                String ext = Path.GetExtension(model.Image.FileName);
                if ( ! IsAllowedFileType(ext))
                {
                    return "Тип файлу не є допустимим.";
                }
                
                // змінюємо ім'я файлу - це запобігає випадковому перезапису
                newName = Guid.NewGuid().ToString() + ext;

                // зберігаємо файл - в окрему папку wwwroot/uploads (створюємо вручну)
                model.Image.CopyTo(new FileStream($"wwwroot/uploads/{newName}", FileMode.Create));
            }
            else { return "Файл-картинка необхідні"; }

            // додаємо користувача до БД
            _dataContext.Products.Add(new Data.Entity.Product
            {
                ID = Guid.NewGuid(),
                Title = model.Title,               
                Description = model.Description,
                ProductGroupID = model.ProductGroupId,
                CreateDt = DateTime.Now,
                Price = model.Price,
                ImageUrl = newName
            });
            // зберігаємо внесені зміни
            _dataContext.SaveChanges(); // PlanetScale не підтримує асинхронні запити
            return String.Empty;
        }
        static bool IsAllowedFileType(string fileExtension)
        {
            // Описуйте допустимі типи файлів у вашому переліку
            string[] allowedExtensions = { ".jpg", ".png", ".gif" };

            return Array.Exists(allowedExtensions, ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
    }
}

// Д.З. Перевірити тип файлу (картинка товару) на допустимий перелік
// Доповнити валідацію (ціна бульна за 0 і т.п.)
// Наповнити БД Товарами (із розподілом по групам)
