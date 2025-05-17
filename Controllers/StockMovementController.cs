using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System;

namespace InventorySystem.Controllers
{
    [Authorize]
    public class StockMovementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockMovementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StockMovement
        public async Task<IActionResult> Index()
        {
            var movements = await _context.StockMovements
                .Include(s => s.Item)
                .Include(s => s.User)
                .OrderByDescending(s => s.MovementDate)
                .ToListAsync();
            return View(movements);
        }

        // GET: StockMovement/Create
        public async Task<IActionResult> Create()
        {
            // Still allow viewing the page, but add a message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to modify inventory. Only administrators can create stock movements.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                ViewBag.Items = (await _context.Items.OrderBy(i => i.Name).ToListAsync())
                    .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name ?? "[Unknown]" })
                    .ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Items = new List<SelectListItem>();
                TempData["Error"] = "Could not load items from the database. Please check your database connection. Error: " + ex.Message;
            }
            return View(new StockMovement());
        }

        // POST: StockMovement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,Type,Quantity,Notes")] StockMovement movement)
        {
            // Check for admin permission
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to modify inventory. Only administrators can create stock movements.";
                return RedirectToAction(nameof(Index));
            }

            // Skip the usual ItemId validation - we'll handle it manually
            // as the default validation seems to conflict with our implementation
            ModelState.Remove("ItemId");  
            ModelState.Remove("Item");
            
            // Manually validate ItemId instead
            if (movement.ItemId <= 0)
            {
                ModelState.AddModelError("ItemId", "Please select an item.");
            }
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] ModelState error for {key}: {error.ErrorMessage}");
                    }
                }
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    TempData["Error"] = error.ErrorMessage;
                }
                ViewBag.Items = (await _context.Items.OrderBy(i => i.Name).ToListAsync())
                    .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name })
                    .ToList();
                return View(movement);
            }

            try
            {
                movement.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(movement.UserId))
                {
                    TempData["Error"] = "Could not determine the current user. Please log in again.";
                    ViewBag.Items = (await _context.Items.OrderBy(i => i.Name).ToListAsync())
                        .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name })
                        .ToList();
                    return View(movement);
                }
                movement.MovementDate = DateTime.Now;
                
                // Ensure Item exists
                var item = await _context.Items.FindAsync(movement.ItemId);
                if (item == null)
                {
                    TempData["Error"] = "The selected item does not exist.";
                    ViewBag.Items = (await _context.Items.OrderBy(i => i.Name).ToListAsync())
                        .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name ?? "[Unknown]" })
                        .ToList();
                    return View(movement);
                }
                
                _context.Add(movement);

                // Update item quantity
                // We already found the item above
                if (item != null)
                {
                    // Apply the effect on stock levels
                    switch (movement.Type)
                    {
                        case MovementType.In:
                            item.Quantity += movement.Quantity;
                            await CreateAlert($"Stock increased by {movement.Quantity} for {item.Name ?? "Unknown Item"}", AlertType.Info, item.Id);
                            break;
                        case MovementType.Out:
                            // Check if there's enough stock
                            if (item.Quantity < movement.Quantity)
                            {
                                TempData["Error"] = $"Not enough stock. Available: {item.Quantity}";
                                ViewBag.Items = (await _context.Items.OrderBy(i => i.Name).ToListAsync())
                                    .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name })
                                    .ToList();
                                return View(movement);
                            }
                            item.Quantity -= movement.Quantity;
                            await CreateAlert($"Stock decreased by {movement.Quantity} for {item.Name ?? "Unknown Item"}", AlertType.Info, item.Id);
                            
                            // Check if stock is below reorder level
                            if (item.Quantity <= item.ReorderLevel)
                            {
                                await CreateAlert($"STOCK ALERT: {item.Name ?? "Unknown Item"} is below reorder level. Current quantity: {item.Quantity}", AlertType.Warning, item.Id, "Manager");
                            }
                            
                            // Check if stock is critically low (less than half of reorder level)
                            if (item.Quantity <= item.ReorderLevel / 2)
                            {
                                await CreateAlert($"CRITICAL STOCK ALERT: {item.Name ?? "Unknown Item"} is critically low. Current quantity: {item.Quantity}", AlertType.Danger, item.Id, "Manager");
                            }
                            break;
                        case MovementType.Adjustment:
                            int oldQuantity = item.Quantity;
                            item.Quantity = movement.Quantity;
                            await CreateAlert($"Stock adjusted from {oldQuantity} to {movement.Quantity} for {item.Name ?? "Unknown Item"}", AlertType.Warning, item.Id, "Manager");
                            break;
                    }
                    
                    item.UpdatedAt = DateTime.Now;
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the stock movement: " + ex.Message;
                ViewBag.Items = (await _context.Items.OrderBy(i => i.Name).ToListAsync())
                    .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name })
                    .ToList();
                return View(movement);
            }
        }

        // GET: StockMovement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Still allow viewing the page, but add a message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to modify inventory. Only administrators can edit stock movements.";
                return RedirectToAction(nameof(Index));
            }
            
            if (id == null)
            {
                return NotFound();
            }

            var movement = await _context.StockMovements.FindAsync(id);
            if (movement == null)
            {
                return NotFound();
            }

            ViewBag.Items = await _context.Items.OrderBy(i => i.Name).ToListAsync();
            return View(movement);
        }

        // POST: StockMovement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ItemId,Type,Quantity,Notes")] StockMovement movement)
        {
            // Check for admin permission
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to modify inventory. Only administrators can edit stock movements.";
                return RedirectToAction(nameof(Index));
            }
            
            if (id != movement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalMovement = await _context.StockMovements
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (originalMovement != null)
                    {
                        // Revert the original movement's effect
                        var item = await _context.Items.FindAsync(originalMovement.ItemId);
                        if (item != null)
                        {
                            switch (originalMovement.Type)
                            {
                                case MovementType.In:
                                    item.Quantity -= originalMovement.Quantity;
                                    break;
                                case MovementType.Out:
                                    item.Quantity += originalMovement.Quantity;
                                    break;
                                case MovementType.Adjustment:
                                    // For adjustment, we need to know the previous quantity
                                    // This would require additional tracking
                                    break;
                            }
                        }
                    }

                    // Apply the new movement
                    var newItem = await _context.Items.FindAsync(movement.ItemId);
                    if (newItem != null)
                    {
                        switch (movement.Type)
                        {
                            case MovementType.In:
                                newItem.Quantity += movement.Quantity;
                                break;
                            case MovementType.Out:
                                newItem.Quantity -= movement.Quantity;
                                break;
                            case MovementType.Adjustment:
                                newItem.Quantity = movement.Quantity;
                                break;
                        }
                        newItem.UpdatedAt = DateTime.Now;
                    }

                    _context.Update(movement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockMovementExists(movement.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Items = await _context.Items.OrderBy(i => i.Name).ToListAsync();
            return View(movement);
        }

        // GET: StockMovement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movement = await _context.StockMovements
                .Include(s => s.Item)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movement == null)
            {
                return NotFound();
            }

            return View(movement);
        }

        private bool StockMovementExists(int id)
        {
            return _context.StockMovements.Any(e => e.Id == id);
        }
        
        // Helper method to create alerts
        private async Task CreateAlert(string message, AlertType type, int? itemId = null, string? targetRole = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var alert = new Alert
            {
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now,
                TargetRole = targetRole,
                UserId = targetRole == null ? userId : null, // If targeted to a role, don't assign to specific user
                ItemId = itemId
            };

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
        }
    }
}