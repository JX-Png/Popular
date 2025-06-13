using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace PopularBookstore.Pages
{
    [Authorize]
    public class CartModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}