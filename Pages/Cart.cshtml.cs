using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PopularBookstore.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Data;

namespace PopularBookstore.Pages
{
    public class CartModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const string CartCookieKey = "MyCart";

        public CartModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalCost { get; set; }

        // This method runs when the user navigates to the cart page.
        // It reads the simple cookie and fetches full book details to display.
        public async Task OnGetAsync()
        {
            var cart = GetCartFromCookie();
            foreach (var item in cart)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book != null)
                {
                    CartItems.Add(new CartItem
                    {
                        BookId = book.Id,
                        Quantity = item.Quantity,
                        Title = book.Title,
                        Price = book.Price,
                        ImageData = book.ImageData,
                        ImageMimeType = book.ImageMimeType
                    });
                }
            }
            CalculateTotal();
        }

        // This handler ONLY modifies the cookie. It's fast and simple.
        public IActionResult OnGetAddToCart(int id)
        {
            var cart = GetCartFromCookie();
            var item = cart.FirstOrDefault(c => c.BookId == id);

            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartCookieItem { BookId = id, Quantity = 1 });
            }

            SaveCartToCookie(cart);
            return RedirectToPage("/Cart"); // Redirect to the cart page to show the result
        }

        // This handler also ONLY modifies the cookie.
        public IActionResult OnPostRemoveFromCart(int id)
        {
            var cart = GetCartFromCookie();
            var itemToRemove = cart.FirstOrDefault(c => c.BookId == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            SaveCartToCookie(cart);
            return RedirectToPage();
        }

        private List<CartCookieItem> GetCartFromCookie()
        {
            var cookieValue = Request.Cookies[CartCookieKey];
            if (string.IsNullOrEmpty(cookieValue))
            {
                return new List<CartCookieItem>();
            }
            return JsonSerializer.Deserialize<List<CartCookieItem>>(cookieValue) ?? new List<CartCookieItem>();
        }

        private void SaveCartToCookie(List<CartCookieItem> cart)
        {
            var cookieValue = JsonSerializer.Serialize(cart);
            Response.Cookies.Append(CartCookieKey, cookieValue, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = System.DateTime.Now.AddDays(7),
                IsEssential = true
            });
        }

        private void CalculateTotal()
        {
            TotalCost = CartItems.Sum(item => item.Price * item.Quantity);
        }
    }
}