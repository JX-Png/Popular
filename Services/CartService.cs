using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace PopularBookstore.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
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