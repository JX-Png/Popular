using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization; // Add this

namespace PopularBookstore.Pages // Corrected namespace
{
    [Authorize]
    public class PromotionsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}