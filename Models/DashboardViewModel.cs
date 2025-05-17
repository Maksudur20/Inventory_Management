using System.Collections.Generic;

namespace InventorySystem.Models
{
    public class DashboardViewModel
    {
        public int TotalItems { get; set; }
        public int LowStockCount { get; set; }
        public int TotalMovements { get; set; }
        public List<StockMovement> RecentMovements { get; set; } = new List<StockMovement>();
        public List<Alert> UnreadAlerts { get; set; } = new List<Alert>();
    }
}
