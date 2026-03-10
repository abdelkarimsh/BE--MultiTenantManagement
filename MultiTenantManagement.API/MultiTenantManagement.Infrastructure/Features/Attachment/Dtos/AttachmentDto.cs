namespace MultiTenantManagement.Infrastructure.Features.Attachment.Dtos
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string FileKey { get; set; } = string.Empty;
        public string StorageProvider { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Category { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public DateTime UploadedAtUtc { get; set; }
    }
}
