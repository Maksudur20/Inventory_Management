using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.Models;
using InventorySystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace InventorySystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        // Get the user and their role
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        // Create dashboard view model
        var dashboard = new DashboardViewModel
        {
            TotalItems = await _context.Items.CountAsync(),
            LowStockCount = await _context.Items.CountAsync(i => i.Quantity <= i.ReorderLevel),
            TotalMovements = await _context.StockMovements.CountAsync(),
            RecentMovements = await _context.StockMovements
                .Include(s => s.Item)
                .Include(s => s.User)
                .OrderByDescending(s => s.MovementDate)
                .Take(5)
                .ToListAsync(),
            UnreadAlerts = await _context.Alerts
                .Where(a => !a.IsRead && (a.UserId == userId || a.TargetRole == userRole || a.TargetRole == null))
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync()
        };
        
        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
