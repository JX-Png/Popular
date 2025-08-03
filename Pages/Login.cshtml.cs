using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PopularBookstore.Services;
using PopularBookstore.ViewModels;
using WebApplication1.Models; // Add this namespace

namespace PopularBookstore.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager; // Changed from IdentityUser
        private readonly ILogger<LoginModel> _logger;
        private readonly CartService _cartService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, CartService cartService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _cartService = cartService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // Merge carts after successful login
                    MergeCarts();

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }

        private void MergeCarts()
        {
            var anonymousCartCookieName = "cart-items-anonymous";
            var userCartCookieName = _cartService.GetCartCookieName();

            var anonymousCartJson = Request.Cookies[anonymousCartCookieName];
            if (string.IsNullOrEmpty(anonymousCartJson))
            {
                return; // No anonymous cart to merge
            }

            var anonymousCart = JsonSerializer.Deserialize<List<CartCookieItem>>(anonymousCartJson) ?? new List<CartCookieItem>();
            if (!anonymousCart.Any())
            {
                return;
            }

            var userCartJson = Request.Cookies[userCartCookieName];
            var userCart = new List<CartCookieItem>();
            if (!string.IsNullOrEmpty(userCartJson))
            {
                userCart = JsonSerializer.Deserialize<List<CartCookieItem>>(userCartJson) ?? new List<CartCookieItem>();
            }

            // Merge items
            foreach (var anonymousItem in anonymousCart)
            {
                var userItem = userCart.FirstOrDefault(i => i.BookId == anonymousItem.BookId);
                if (userItem != null)
                {
                    userItem.Quantity += anonymousItem.Quantity; // Combine quantities
                }
                else
                {
                    userCart.Add(anonymousItem); // Add new item
                }
            }

            // Save the merged cart to the user's cookie
            Response.Cookies.Append(userCartCookieName, JsonSerializer.Serialize(userCart), new CookieOptions
            {
                Expires = System.DateTime.Now.AddDays(30)
            });

            // Clear the anonymous cart cookie
            Response.Cookies.Delete(anonymousCartCookieName);
        }
    }
}