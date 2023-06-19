using ASP121.Data;
using ASP121.Models.Rate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ASP121.Controllers
{
    [Route("api/rate")]
    [ApiController]
    public class RateController : ControllerBase
    {
        private readonly DataContext _dataContext;

		public RateController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}

		[HttpPost]
        public object DoPost([FromBody] RateModel model)
        {
            // шукаємо чи давав даний користувач оцінку даному товару
            var rate = _dataContext.Rates
                .Where(r => r.ProductID == model.ProductID
                         && r.UserID == model.UserID)
                .FirstOrDefault();
            if (rate != null) // була оцінка
            {
                return new { Status = false, Message = "Оцінка була дана раніше" };
            }
            else // нова оцінка
            {
                _dataContext.Rates.Add(new()
                {
                    ProductID = model.ProductID,
                    UserID = model.UserID,
                    Rating = model.Rating,
                    Moment = DateTime.Now,
                });
                _dataContext.SaveChanges();
                // оновлюємо дані про рейтинг даного товару
                var product = _dataContext.Products
                    .Include(p => p.Rates)
                    .Where(p => p.ID == model.ProductID)
                    .First();
                // включаємо оновлені дані у відповідь (для відображеня)
                return new {
                    Status = true,
                    Message = "ОК",
                    Positive = product.Rates.Count(r => r.Rating > 0),
					Negative = product.Rates.Count(r => r.Rating < 0)
				};
			}

            // return model;
            // return new { Status = true };
        }
    }
}
/* Д.З. Ркалізувати відображення рейтінгів товарів
 * - створити навігаційну властивість у Entity.Product
 * - конфігурувати її у контексті (OnModelCreating)
 *      !! якщо будемо робити міграцію. то видаляйте / коментуйте
 *      усі інструкції з ForeignKey (Planeta не підтримує)
 * - заповнити її при запиті у контроллері (.Include)
 * - у представленні відобразити реальні числа
 * ** при новій оцінці оновлювати лічильник (рейтинг) якщо від 
 *      бекенду надійде позитивний статус (без оновлення сторінки)
 *      
 * = для роботи з Azure буде потрібна платіжна карткаб створити
 *      окрему картку (Інтернет-), перевести на неї близько $1
 */
