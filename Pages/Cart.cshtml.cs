using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PopularBookstore.Services;
using PopularBookstore.ViewModels; 
using WebApplication1.Data;

namespace PopularBookstore.Pages
{
    public class CartModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CartModel(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalCost { get; set; }

        public void OnGet()
        {
            LoadCart();
        }

        public IActionResult OnPostRemoveFromCart(int id)
        {
            var cartCookieName = _cartService.GetCartCookieName();
            var cartJson = Request.Cookies[cartCookieName];
            var cart = new List<CartCookieItem>();

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = JsonSerializer.Deserialize<List<CartCookieItem>>(cartJson) ?? new List<CartCookieItem>();
            }

            var itemToRemove = cart.FirstOrDefault(item => item.BookId == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                Response.Cookies.Append(cartCookieName, JsonSerializer.Serialize(cart), new CookieOptions
                {
                    Expires = System.DateTime.Now.AddDays(30)
                });
            }

            return RedirectToPage();
        }

        private void LoadCart()
        {
            var cartCookieName = _cartService.GetCartCookieName();
            var cartJson = Request.Cookies[cartCookieName];
            if (string.IsNullOrEmpty(cartJson))
            {
                return;
            }

            var cartCookieItems = JsonSerializer.Deserialize<List<CartCookieItem>>(cartJson) ?? new List<CartCookieItem>();
            var bookIds = cartCookieItems.Select(c => c.BookId).ToList();
            var books = _context.Books.Where(b => bookIds.Contains(b.Id)).ToList();

            CartItems = cartCookieItems.Select(cookieItem =>
            {
                var book = books.FirstOrDefault(b => b.Id == cookieItem.BookId);
                return new CartItem
                {
                    BookId = cookieItem.BookId,
                    Quantity = cookieItem.Quantity,
                    Title = book?.Title ?? "Unknown",
                    Price = book?.Price ?? 0,
                    ImageData = book?.ImageData,
                    ImageMimeType = book?.ImageMimeType
                };
            }).ToList();

            TotalCost = CartItems.Sum(item => item.Price * item.Quantity);
        }
    }
}