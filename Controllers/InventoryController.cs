using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace InventorySystem.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items.ToListAsync();
            return View(items);
        }

        // GET: Inventory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Inventory/Create
        public IActionResult Create()
        {
            // Check permission and redirect with message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to create inventory items. Only administrators can modify inventory.";
                return RedirectToAction(nameof(Index));
            }
            
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Quantity,SKU,ReorderLevel")] Item item)
        {
            // Check permission and redirect with message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to create inventory items. Only administrators can modify inventory.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    // Log the error or add it to TempData for display
                    TempData["Error"] = error.ErrorMessage;
                }
                return View(item);
            }

            try
            {
                item.CreatedAt = DateTime.Now;
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["Error"] = "An error occurred while creating the item: " + ex.Message;
                return View(item);
            }
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Check permission and redirect with message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to edit inventory items. Only administrators can modify inventory.";
                return RedirectToAction(nameof(Index));
            }
            
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Quantity,SKU,ReorderLevel")] Item item)
        {
            // Check permission and redirect with message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to edit inventory items. Only administrators can modify inventory.";
                return RedirectToAction(nameof(Index));
            }
            
            if (id != item.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    TempData["Error"] = error.ErrorMessage;
                }
                return View(item);
            }

            try
            {
                item.UpdatedAt = DateTime.Now;
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(item.Id))
                {
                    return NotFound();
                }
                else
                {
                    TempData["Error"] = "The item was modified by another user. Please refresh and try again.";
                    return View(item);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the item: " + ex.Message;
                return View(item);
            }
        }

        // GET: Inventory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Check permission and redirect with message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to delete inventory items. Only administrators can modify inventory.";
                return RedirectToAction(nameof(Index));
            }
            
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check permission and redirect with message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to delete inventory items. Only administrators can modify inventory.";
                return RedirectToAction(nameof(Index));
            }
            
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }

        public async Task<IActionResult> LowStock()
        {
            var lowStock = await _context.Items
                .Where(i => i.Quantity <= i.ReorderLevel)
                .ToListAsync();

            return View(lowStock);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int itemId, int quantity)
        {
            // Check permission and redirect with message for non-admins
            if (!User.IsInRole("Administrator"))
            {
                TempData["Error"] = "You do not have permission to update stock levels. Only administrators can modify inventory.";
                return RedirectToAction(nameof(Index));
            }
            
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item != null)
            {
                item.Quantity = quantity;
                item.UpdatedAt = DateTime.Now;

                _context.StockMovements.Add(new StockMovement
                {
                    ItemId = item.Id,
                    Quantity = quantity - item.Quantity,
                    Type = MovementType.Adjustment,
                    Notes = "Manual stock update",
                    UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}