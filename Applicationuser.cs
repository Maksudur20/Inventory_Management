using Microsoft.AspNetCore.Identity;

namespace InventorySystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Role { get; set; }
    }
}
