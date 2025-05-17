using Microsoft.AspNetCore.Mvc;
using InventorySystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace InventorySystem.ViewComponents
{
    public class AlertCounterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public AlertCounterViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return View(0);
            }

            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userRole = UserClaimsPrincipal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            // Get count of unread alerts for this user specifically or for their role
            var count = await _context.Alerts
                .Where(a => !a.IsRead && (a.UserId == userId || a.TargetRole == userRole || a.TargetRole == null))
                .CountAsync();

            return View(count);
        }
    }
}
