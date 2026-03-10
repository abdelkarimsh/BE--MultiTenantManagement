using System.ComponentModel.DataAnnotations;

namespace MultiTenantManagement.Data.Models
{
    public  class Tenant
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        [Required, MaxLength(64)]
        public required string Status { get; set; }

        [MaxLength(256)]
        public string? LogoUrl { get; set; }

        public Guid? AttachmentId { get; set; }

        [Required, MaxLength(250)]
        public required string SubDomain { get; set; } 

        // Navigation
        public ICollection<ApplicationUser> Users { get; init; } = new List<ApplicationUser>();
        public ICollection<Product> Products { get; init; } = new List<Product>();
        public ICollection<Order> Orders { get; init; } = new List<Order>();
        public StoreSetting? StoreSetting { get; set; }
        public Attachment? Attachment { get; set; }
    }
}
