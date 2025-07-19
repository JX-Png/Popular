using Microsoft.AspNetCore.Mvc;
using PopularBookstore.Services;
using PopularBookstore.ViewModels; 
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PopularBookstore.ViewComponents
{
    public class CartWidgetViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartWidgetViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var cartCookieName = _cartService.GetCartCookieName();
            var cartJson = Request.Cookies[cartCookieName];
            var cartItemCount = 0;

            if (!string.IsNullOrEmpty(cartJson))
            {
                var cart = JsonSerializer.Deserialize<List<CartCookieItem>>(cartJson);
                if (cart != null)
                {
                    cartItemCount = cart.Sum(item => item.Quantity);
                }
            }

            return View(cartItemCount);
        }
    }
}