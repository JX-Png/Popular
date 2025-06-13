using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PopularBookstore.Pages // Ensure this namespace matches your project
{
    [AllowAnonymous] // Allow even unauthenticated users to reach this to perform logout
    public class LogoutModel : PageModel
    {
        // This handler is called when the form (with the logout button) is POSTed.
        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login"); // Redirect to the login page after logout
        }

        // Optional: Handle GET requests to /Logout, perhaps by redirecting or showing a message.
        // For a button-triggered logout, OnPostAsync is the primary concern.
        public IActionResult OnGet()
        {
            // If you want to prevent direct GET access or handle it differently:
            return RedirectToPage("/Index"); // Or RedirectToPage("/Login");
        }
    }
}