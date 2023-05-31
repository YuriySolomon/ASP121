using ASP121.Data;
using ASP121.Models.User;
using ASP121.Services.Hash;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.RegularExpressions;

namespace ASP121.Controllers
{
    public class UserController : Controller
    {
        // Підключення БД - інжекція залежності від контексту (зараєстрованого у Program.cs)
        private readonly DataContext _dataContext;
        private readonly IHashService _hashService;

        public UserController(DataContext dataContext, IHashService hashService)
        {
            _dataContext = dataContext;
            _hashService = hashService;
        }

        public IActionResult SignUp(UserSignupModel? model)
        {
            if (HttpContext.Request.Method == "POST") // є передані з форми дані
            {
                ViewData["form"] = _ValidateModel(model);
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult LogIn([FromForm] String login, [FromForm] String password)
        {
            /* Використовуючи _dataContext виявити чи є користувач із 
             * переданими login та password (пароль збрігається як геш)
             * В залежності від результату перевірки надіслати відповідь
             * Json(new { status = "OK" }) або
             * Json(new { status = "NO" }) 
             */
            var user = _dataContext.Users121.FirstOrDefault(u => u.Login == login);
            if (user != null)
            {
                if (user.PasswordHash == _hashService.HashString(password))
                {
                    // Автентифікація пройдена
                    // Зберігаємо у сесії id користувача
                    HttpContext.Session.SetString("AuthUserId", user.ID.ToString());
                    return Json(new { status = "OK" });
                }
            }            
            return Json(new { status = "NO" });
            //return Json(new { login, password });
        }

        // Перевіряє валідність даних у моделі, прийнятої з форми
        // Повертає повідомлення про помилку валідації або Sting.Empty
        // якщо перевірка успішна
        private String _ValidateModel(UserSignupModel? model)
        {
            if (model == null) { return "Дані не передані"; }
            if (String.IsNullOrEmpty(model.Login)) { return "Логін не може бути порожнім"; }
            var user = _dataContext.Users121.FirstOrDefault(u => u.Login == model.Login);
            if (user != null) { return "Такий логін вже існує"; }

                if (String.IsNullOrEmpty(model.Password) || String.IsNullOrEmpty(model.RepeatPassword)) { return "Пароль не може бути порожнім"; }
            if (model.Password != model.RepeatPassword) { return "Паролі не співпадають"; }

            if (String.IsNullOrEmpty(model.Email)) { return "Email не може бути порожнім"; }
            string email = model.Email;
            if ( ! IsValidEmail(email))
            {
                return "Email не є коректним.";
            }

            if (! model.Agree) { return "необхідно дотримуватися павил сайту"; }

            // завантажуємо аватарку

            String? newName = null;
            if (model.AvatarFile != null)  // є файл
            {
                // визначаємо тип (розширення) файлу
                String ext = Path.GetExtension(model.AvatarFile.FileName);
                // Д.З. перевірити тип файлу на допустимий перелік
                if ( ! IsAllowedFileType(ext))
                {
                    return "Тип файлу не є допустимим.";
                }

                // змінюємо ім'я файлу - це запобігає випадковому перезапису
                newName = Guid.NewGuid().ToString() + ext;

                // зберігаємо файл - в окрему папку wwwroot/uploads (створюємо вручну)
                model.AvatarFile.CopyTo(new FileStream($"wwwroot/uploads/{newName}", FileMode.Create));
            }

            // додаємо користувача до БД
            _dataContext.Users121.Add(new Data.Entity.User
            {
                ID = Guid.NewGuid(),
                Login = model.Login,
                //PasswordHash = model.Password,
                PasswordHash = _hashService.HashString(model.Password),
                Email = model.Email,
                Avatar = newName,
                RealName= model.RealName,
                RegisteredDt = DateTime.Now
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

        static bool IsValidEmail(string email)
        {
            // Регулярний вираз для перевірки формату Email
            string pattern = @"^[\w\.-]+@([\w-]+\.)+[\w-]{2,4}$";

            return Regex.IsMatch(email, pattern);
        }

    }
}

/* Д.З. Валідація:
 *          - додати перевірку на однаковість паролю та його повтору
 *          - перевірити тип файлу на допустимий перелік
 *          - * перевірити Email регулярним виразом
 *          - * перевірити пароль на складність
 *          - перевірити Логін на те, що такий вже використовується
 */
