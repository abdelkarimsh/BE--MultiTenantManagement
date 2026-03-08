using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MultiTenantManagement.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? TenantId { get; set; }
        public Tenant? Tenant { get; init; }
        public bool IsDeleted { get; set; }
        [MaxLength(128)]
        public string? FullName { get; set; }
        [MaxLength(32)]
        public string? Role { get; set; }
        public DateTime  CreatedAtUtc { get; init; } = DateTime.UtcNow;
    }
}
