using Microsoft.AspNetCore.Mvc;
using PopularBookstore.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace WebApplication1.ViewComponents
{
    public class CartWidgetViewComponent : ViewComponent
    {
        private const string CartCookieKey = "MyCart";

        public IViewComponentResult Invoke()
        {
            var totalItems = 0;
            var cookieValue = Request.Cookies[CartCookieKey];

            if (!string.IsNullOrEmpty(cookieValue))
            {
                var items = JsonSerializer.Deserialize<List<CartCookieItem>>(cookieValue);
                if (items != null)
                {
                    totalItems = items.Sum(item => item.Quantity);
                }
            }

            return View(totalItems);
        }
    }
}