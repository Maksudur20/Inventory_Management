using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;

namespace InventorySystem.Controllers
{
    [Authorize]
    public class AlertController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlertController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alert
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            // Get alerts for this user specifically or for their role
            var alerts = await _context.Alerts
                .Include(a => a.Item)
                .Include(a => a.User)
                .Where(a => a.UserId == userId || a.TargetRole == userRole || a.TargetRole == null)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
                
            return View(alerts);
        }

        // POST: Alert/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            alert.IsRead = true;
            _context.Update(alert);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Alert/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            var alerts = await _context.Alerts
                .Where(a => (a.UserId == userId || a.TargetRole == userRole || a.TargetRole == null) && !a.IsRead)
                .ToListAsync();
                
            foreach (var alert in alerts)
            {
                alert.IsRead = true;
                _context.Update(alert);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper method to create alerts - can be called from other controllers
        public static async Task CreateAlertAsync(ApplicationDbContext context, string message, AlertType type, 
            string? targetRole = null, string? userId = null, int? itemId = null)
        {
            var alert = new Alert
            {
                Message = message,
                Type = type,
                CreatedAt = System.DateTime.Now,
                TargetRole = targetRole,
                UserId = userId,
                ItemId = itemId
            };

            context.Alerts.Add(alert);
            await context.SaveChangesAsync();
        }
    }
}
