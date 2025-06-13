using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace PopularBookstore.Pages
{
    [Authorize]
    public class OrderHistoryModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}