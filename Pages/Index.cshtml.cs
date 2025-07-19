using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PopularBookstore.Services;
using PopularBookstore.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WebApplication1.Data;
using WebApplication1.Models;

namespace PopularBookstore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public IndexModel(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public List<Book> Books { get; set; } = new List<Book>();

        public async Task OnGetAsync()
        {
            Books = await _context.Books.ToListAsync();
        }

        public IActionResult OnPostAddToCart(int id)
        {
            var cartCookieName = _cartService.GetCartCookieName();
            var cartJson = Request.Cookies[cartCookieName];
            var cart = new List<CartCookieItem>();

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = JsonSerializer.Deserialize<List<CartCookieItem>>(cartJson) ?? new List<CartCookieItem>();
            }

            var existingItem = cart.FirstOrDefault(item => item.BookId == id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartCookieItem { BookId = id, Quantity = 1 });
            }

            Response.Cookies.Append(cartCookieName, JsonSerializer.Serialize(cart), new CookieOptions
            {
                Expires = System.DateTime.Now.AddDays(30)
            });

            return RedirectToPage();
        }
    }
}