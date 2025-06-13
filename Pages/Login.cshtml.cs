using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

public class LoginModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/Index");
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/Index");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // --- Mock Authentication ---
        // In a real app, you'd validate against a database (e.g., using ASP.NET Core Identity)
        // For now, let's assume the user created via "Create Account" was:
        // Email: user@example.com (from your previous dummy check)
        // Password: password (from your previous dummy check)
        // OR a user you "created" via the CreateAccount page.
        // Let's use a predefined user for simplicity here.
        if (Input.Email == "test@example.com" && Input.Password == "Password123!")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Input.Email),
                new Claim(ClaimTypes.Email, Input.Email),
                // Add other claims as needed, e.g., roles
                // new Claim(ClaimTypes.Role, "User"),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie is persistent or session-only.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return LocalRedirect(ReturnUrl);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}