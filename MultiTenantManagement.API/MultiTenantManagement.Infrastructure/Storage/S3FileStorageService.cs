using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace MultiTenantManagement.Infrastructure.Storage;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3StorageSettings _settings;

    public S3FileStorageService(IAmazonS3 s3Client, IOptions<S3StorageSettings> options)
    {
        _s3Client = s3Client;
        _settings = options.Value;
    }

    public async Task<FileStorageUploadResult> UploadAsync(FileStorageUploadRequest request, CancellationToken ct = default)
    {
        if (request.Content == Stream.Null)
            throw new ArgumentException("File content stream is required.", nameof(request));

        var fileKey = BuildFileKey(request);
        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileKey,
            InputStream = request.Content,
            ContentType = request.ContentType
        };

        await _s3Client.PutObjectAsync(putRequest, ct);

        return new FileStorageUploadResult
        {
            TenantId = request.TenantId,
            OriginalFileName = request.OriginalFileName,
            StoredFileName = request.StoredFileName,
            FileKey = fileKey,
            ContentType = request.ContentType,
            Size = request.Size,
            StorageProvider = "S3",
            Url = BuildPublicUrl(fileKey)
        };
    }

    private string BuildFileKey(FileStorageUploadRequest request)
    {
        return $"tenants/{request.TenantId:D}/{request.Category}/{request.EntityType}/{request.EntityId}/{request.StoredFileName}";
    }

    private string BuildPublicUrl(string key)
    {
        if (!string.IsNullOrWhiteSpace(_settings.PublicBaseUrl))
        {
            return $"{_settings.PublicBaseUrl.TrimEnd('/')}/{key}";
        }

        return $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{key}";
    }
}
