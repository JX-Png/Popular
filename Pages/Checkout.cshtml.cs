using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PopularBookstore.Services;
using PopularBookstore.ViewModels;
using WebApplication1.Data;
using WebApplication1.Models;

namespace PopularBookstore.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;

        public CheckoutModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public List<PaymentCard> PaymentCards { get; set; } = new List<PaymentCard>();
        public decimal TotalAmount { get; set; }
        public bool OrderCompleted { get; set; }
        public int OrderId { get; set; }

        [BindProperty]
        public int? SelectedPaymentCardId { get; set; }

        [BindProperty]
        public ShippingAddressModel ShippingAddress { get; set; } = new ShippingAddressModel();

        public class ShippingAddressModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [Display(Name = "Address")]
            public string Address { get; set; }

            [Required]
            [Display(Name = "City")]
            public string City { get; set; }

            [Required]
            [Display(Name = "Postal Code")]
            public string PostalCode { get; set; }

            [Required]
            [Display(Name = "Country")]
            public string Country { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login", new { returnUrl = "/Checkout" });
            }

            // Load cart items
            await LoadCartItems();

            if (CartItems.Count == 0)
            {
                return RedirectToPage("/Cart");
            }

            // Load user's payment cards
            var user = await _userManager.GetUserAsync(User);
            PaymentCards = await _context.PaymentCards
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.IsDefault)
                .ToListAsync();

            // Pre-select default card if available
            var defaultCard = PaymentCards.FirstOrDefault(c => c.IsDefault);
            if (defaultCard != null)
            {
                SelectedPaymentCardId = defaultCard.Id;
            }

            // Parse user address into components if available
            if (!string.IsNullOrEmpty(user.Address))
            {
                // Try to parse the address - assume format: street address, city, postal code, country
                var addressParts = user.Address.Split(',', StringSplitOptions.RemoveEmptyEntries);

                ShippingAddress = new ShippingAddressModel
                {
                    FullName = user.Name,
                    Address = addressParts.Length > 0 ? addressParts[0].Trim() : user.Address,
                    City = addressParts.Length > 1 ? addressParts[1].Trim() : "",
                    PostalCode = addressParts.Length > 2 ? addressParts[2].Trim() : "",
                    Country = addressParts.Length > 3 ? addressParts[3].Trim() : "Singapore"
                };
            }
            else
            {
                ShippingAddress = new ShippingAddressModel
                {
                    FullName = user.Name,
                    Address = "",
                    City = "",
                    PostalCode = "",
                    Country = "Singapore" // Default country
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login", new { returnUrl = "/Checkout" });
            }

            // Load cart items
            await LoadCartItems();

            if (CartItems.Count == 0)
            {
                return RedirectToPage("/Cart");
            }

            // Get the user
            var user = await _userManager.GetUserAsync(User);

            // Validate payment method
            if (!SelectedPaymentCardId.HasValue)
            {
                ModelState.AddModelError("SelectedPaymentCardId", "Please select a payment method.");
                await LoadPaymentCards(user.Id);
                return Page();
            }

            var paymentCard = await _context.PaymentCards
                .FirstOrDefaultAsync(c => c.Id == SelectedPaymentCardId.Value && c.UserId == user.Id);

            if (paymentCard == null)
            {
                ModelState.AddModelError("SelectedPaymentCardId", "Selected payment method is not valid.");
                await LoadPaymentCards(user.Id);
                return Page();
            }

            // Validate shipping information
            if (!ModelState.IsValid)
            {
                await LoadPaymentCards(user.Id);
                return Page();
            }

            // Create a new order
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = TotalAmount,
                Status = "Processing",
                ShippingName = ShippingAddress.FullName,
                ShippingAddress = $"{ShippingAddress.Address}, {ShippingAddress.City}, {ShippingAddress.PostalCode}, {ShippingAddress.Country}",
                PaymentMethod = $"{paymentCard.CardName} ending in {paymentCard.CardNumberLastFour}"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add order items
            foreach (var item in CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };

                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            // Clear the cart
            var cartCookieName = _cartService.GetCartCookieName();
            Response.Cookies.Delete(cartCookieName);

            // Set confirmation values
            OrderCompleted = true;
            OrderId = order.Id;

            return Page();
        }

        private async Task LoadCartItems()
        {
            var cartCookieName = _cartService.GetCartCookieName();
            var cartJson = Request.Cookies[cartCookieName];

            if (string.IsNullOrEmpty(cartJson))
            {
                return;
            }

            var cartItems = JsonSerializer.Deserialize<List<CartCookieItem>>(cartJson) ?? new List<CartCookieItem>();
            var bookIds = cartItems.Select(i => i.BookId).ToList();
            var books = await _context.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync();

            CartItems = cartItems
                .Select(item => {
                    var book = books.FirstOrDefault(b => b.Id == item.BookId);
                    return new CartItemViewModel
                    {
                        BookId = item.BookId,
                        Title = book?.Title ?? "Unknown Book",
                        Author = book?.Author ?? "Unknown Author",
                        Price = book?.Price ?? 0,
                        Quantity = item.Quantity,
                        ImageUrl = book?.ImageUrl,
                        ImageData = book?.ImageData,
                        ImageMimeType = book?.ImageMimeType
                    };
                })
                .ToList();

            TotalAmount = CartItems.Sum(i => i.Price * i.Quantity);
        }

        private async Task LoadPaymentCards(string userId)
        {
            PaymentCards = await _context.PaymentCards
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IsDefault)
                .ToListAsync();
        }
    }
}