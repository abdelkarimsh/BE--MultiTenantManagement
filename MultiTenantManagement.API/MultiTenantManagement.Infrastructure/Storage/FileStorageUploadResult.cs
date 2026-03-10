namespace MultiTenantManagement.Infrastructure.Storage;

public class FileStorageUploadResult
{
    public Guid TenantId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string StorageProvider { get; set; } = "S3";
    public string? Url { get; set; }
}
