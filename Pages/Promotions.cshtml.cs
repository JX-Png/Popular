using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization; 

namespace PopularBookstore.Pages
{
    [Authorize]
    public class PromotionsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}