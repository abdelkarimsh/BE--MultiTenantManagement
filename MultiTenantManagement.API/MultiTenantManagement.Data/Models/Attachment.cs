using System.ComponentModel.DataAnnotations;

namespace MultiTenantManagement.Data.Models
{
    public class Attachment
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        [Required, MaxLength(255)]
        public string OriginalFileName { get; set; } = null!;

        [Required]
        public Guid TenantId { get; set; }

        [Required, MaxLength(255)]
        public string StoredFileName { get; set; } = null!;

        [Required, MaxLength(1024)]
        public string FileKey { get; set; } = null!;

        [Required, MaxLength(50)]
        public string StorageProvider { get; set; } = null!;

        [Required, MaxLength(150)]
        public string ContentType { get; set; } = null!;

        public long Size { get; set; }

        [Required, MaxLength(100)]
        public string Category { get; set; } = null!;

        [Required, MaxLength(100)]
        public string EntityType { get; set; } = null!;

        [Required, MaxLength(100)]
        public string EntityId { get; set; } = null!;

        public DateTime UploadedAtUtc { get; init; } = DateTime.UtcNow;
    }
}
