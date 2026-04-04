namespace MultiTenantManagement.Infrastructure.Storage;

public class StorageSettings
{
    public const string SectionName = "Storage";

    public bool UseS3 { get; set; } = true;
}
