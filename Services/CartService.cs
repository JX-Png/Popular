using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace PopularBookstore.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public CartService(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public string GetCartCookieName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                return $"cart-items-{userId}";
            }

            return "cart-items-anonymous";
        }
    }
}