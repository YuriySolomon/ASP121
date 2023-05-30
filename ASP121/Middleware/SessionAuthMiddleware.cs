using ASP121.Data;
using Microsoft.AspNetCore.Builder;
using System.Security.Claims;

namespace ASP121.Middleware
{
    public class SessionAuthMiddleware
    {
        /* Ланцюг викликів (pipeline) утворюється через те, що кожен клас
         * Middleware викликає наструпний клас. Контейнер ФІЗ передає
         * кожному класу посилання на наступний шар. Клас має зберігти
         * це посилання та використати у своїх кодах
         */
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /* Хоча класи Middleware не є нащадками йкихось базових класів,
         * вони повинні містити саме такий метод: InvokeAsync(HttpContext context)
         * У старих схемах вживається синхронний варіант (Invoke), але
         * він вважається застарілим і не радиться до вживання.
         * Частиною методу має бути виклик наступного шару:  await _next(context)
         * Те. що передує _next, виконується на "прямій" ділянці оброблення
         * запиту, після цієїінструкції - "зворотна" ділянка.
         * Якщо _next не виконувати, то це припинить оброблення запиту. Таке
         * може буьт корисним. якщо подальша робота не можлива, наприклад, 
         * зафіксовано неможливість підключення до БД
         * Оскільки конструктор задіяний для створення ланцюга викликів,
         * Інжекція служб (залежностей) здійснюється через метод
         */
        public async Task InvokeAsync(HttpContext context, DataContext dataContext)
        {
            if(context.Session.Keys.Contains("AuthUserId"))
            {
                // є дані збереженої автентифікації
                // шукаємо користувача за збереженим id
                var user = dataContext.Users121.Find(Guid.Parse(context.Session.GetString("AuthUserId")!));
                if(user != null)
                {
                    // користувач знайдений - заповнюємо параметри
                    // в ASP за це відповідає спеціальна конструкція - Ckaim
                    Claim[] claims = new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Login),
                        new Claim(ClaimTypes.Sid, user.ID.ToString()),
                        new Claim(ClaimTypes.UserData, user.Avatar ?? "")
                    };
                    // У контексті є спеціальне поле User. яке є "збірником" цих параметрів
                    context.User =
                        new ClaimsPrincipal(
                            new ClaimsIdentity(
                                claims, nameof(SessionAuthMiddleware)));
                    // оскільки контекст доступний скрізь у проекті, дані про користувача
                    // також будуть доступні
                }
            }
            
            await _next(context);   // передача роботи наступній ланці
        }
    }

    /* Традицією Middleware є створення розширень, які дозволять у
     * Program.cs підключати цей Middleware за допомогою команди
     * USerXxxx, де Xxxx - це назва Middleware. У нашому випадку - 
     *  app.UseSessionAuth();
     * без розширення це буде команда
     *  app.seMiddleware<SessionAuthMiddleware>();
     */
    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
