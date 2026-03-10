namespace MultiTenantManagement.Infrastructure.Storage;

public class S3StorageSettings
{
    public const string SectionName = "S3Storage";

    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? PublicBaseUrl { get; set; }
}
