using Microsoft.AspNetCore.Hosting;

namespace MultiTenantManagement.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private const string UploadsFolderName = "uploads";

    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<FileStorageUploadResult> UploadAsync(FileStorageUploadRequest request, CancellationToken ct = default)
    {
        if (request.Content == Stream.Null)
            throw new ArgumentException("File content stream is required.", nameof(request));

        var webRootPath = EnsureWebRootPath();
        var uploadsPath = Path.Combine(webRootPath, UploadsFolderName);
        Directory.CreateDirectory(uploadsPath);

        var extension = string.IsNullOrWhiteSpace(request.Extension)
            ? Path.GetExtension(request.StoredFileName)
            : request.Extension;

        var storedFileName = !string.IsNullOrWhiteSpace(request.StoredFileName)
            ? request.StoredFileName
            : $"{Guid.NewGuid():N}{extension}";

        var physicalPath = Path.Combine(uploadsPath, storedFileName);
        await using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await request.Content.CopyToAsync(fileStream, ct);
        }

        var fileKey = $"{UploadsFolderName}/{storedFileName}";

        return new FileStorageUploadResult
        {
            TenantId = request.TenantId,
            OriginalFileName = request.OriginalFileName,
            StoredFileName = storedFileName,
            FileKey = fileKey,
            ContentType = request.ContentType,
            Size = request.Size,
            StorageProvider = "Local",
            Url = $"/{fileKey}"
        };
    }

    public Task DeleteAsync(string fileKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
            throw new ArgumentException("File key is required.", nameof(fileKey));

        var normalizedKey = fileKey.Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(EnsureWebRootPath(), normalizedKey);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    private string EnsureWebRootPath()
    {
        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        Directory.CreateDirectory(webRootPath);
        return webRootPath;
    }
}
