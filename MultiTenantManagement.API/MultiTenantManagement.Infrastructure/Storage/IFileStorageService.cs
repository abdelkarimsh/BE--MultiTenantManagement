namespace MultiTenantManagement.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<FileStorageUploadResult> UploadAsync(FileStorageUploadRequest request, CancellationToken ct = default);
}
