using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace PopularBookstore.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}