using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiTenantManagement.Data;
using MultiTenantManagement.Infrastructure.Features.Attachment.Dtos;
using MultiTenantManagement.Infrastructure.Storage;

namespace MultiTenantManagement.Infrastructure.Features.Attachment
{
    public class AttachmentService : IAttachmentService
    {
        private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".dll", ".bat", ".cmd", ".sh", ".ps1", ".php", ".js", ".html"
        };

        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".png"] = new() { new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
            [".gif"] = new() { new byte[] { 0x47, 0x49, 0x46, 0x38 } },
            [".webp"] = new() { new byte[] { 0x52, 0x49, 0x46, 0x46 } },
            [".pdf"] = new() { new byte[] { 0x25, 0x50, 0x44, 0x46 } }
        };

        private readonly AppDbContext _db;
        private readonly IFileStorageService _fileStorageService;
        private readonly AttachmentOptions _options;

        public AttachmentService(AppDbContext db, IFileStorageService fileStorageService, IOptions<AttachmentOptions> options)
        {
            _db = db;
            _fileStorageService = fileStorageService;
            _options = options.Value;
        }

        public async Task<AttachmentUploadResultDto> UploadAsync(Guid tenantId, AttachmentUploadRequestDto request, CancellationToken ct = default)
        {
            ValidateRequest(request,tenantId);

            var file = request.File;
            var extension = NormalizeExtension(Path.GetExtension(file.FileName));
            var originalFileName = Path.GetFileName(file.FileName);
            var contentType = (file.ContentType ?? string.Empty).Trim().ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";

            await using var fileStream = file.OpenReadStream();
            await ValidateFileSignatureAsync(fileStream, extension, ct);
            fileStream.Position = 0;

            var storageResult = await _fileStorageService.UploadAsync(new FileStorageUploadRequest
            {
                TenantId = tenantId,
                Category = NormalizeSegment(request.Category),
                EntityType = NormalizeSegment(request.EntityType),
                EntityId = NormalizeSegment(request.EntityId),
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                Extension = extension,
                ContentType = contentType,
                Size = file.Length,
                Content = fileStream
            }, ct);

            var attachment = new Data.Models.Attachment
            {
                TenantId = storageResult.TenantId,
                OriginalFileName = storageResult.OriginalFileName,
                StoredFileName = storageResult.StoredFileName,
                FileKey = storageResult.FileKey,
                StorageProvider = storageResult.StorageProvider,
                ContentType = storageResult.ContentType,
                Size = storageResult.Size,
                Category = NormalizeSegment(request.Category),
                EntityType = NormalizeSegment(request.EntityType),
                EntityId = NormalizeSegment(request.EntityId)
            };

            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync(ct);

            return new AttachmentUploadResultDto
            {
                AttachmentId = attachment.Id,
                TenantId = attachment.TenantId,
                OriginalFileName = attachment.OriginalFileName,
                StoredFileName = attachment.StoredFileName,
                FileKey = attachment.FileKey,
                ContentType = attachment.ContentType,
                Size = attachment.Size,
                StorageProvider = attachment.StorageProvider,
                Url = storageResult.Url
            };
        }

        public async Task<AttachmentDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            return await _db.Attachments
                .AsNoTracking()
                .Where(x => x.Id == id && x.TenantId == tenantId)
                .Select(x => new AttachmentDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    OriginalFileName = x.OriginalFileName,
                    StoredFileName = x.StoredFileName,
                    FileKey = x.FileKey,
                    StorageProvider = x.StorageProvider,
                    ContentType = x.ContentType,
                    Size = x.Size,
                    Category = x.Category,
                    EntityType = x.EntityType,
                    EntityId = x.EntityId,
                    UploadedAtUtc = x.UploadedAtUtc
                })
                .FirstOrDefaultAsync(ct);
        }

        private void ValidateRequest(AttachmentUploadRequestDto request,Guid tenantId)
        {
            if (request == null)
                throw new ArgumentException("Upload request is required.");

            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId is required.");

            if (string.IsNullOrWhiteSpace(request.Category))
                throw new ArgumentException("Category is required.");

            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new ArgumentException("EntityType is required.");

            if (string.IsNullOrWhiteSpace(request.EntityId))
                throw new ArgumentException("EntityId is required.");

            var file = request.File;
            if (file == null)
                throw new ArgumentException("File is required.");

            if (file.Length <= 0)
                throw new ArgumentException("Uploaded file is empty.");

            if (_options.MaxFileSizeBytes > 0 && file.Length > _options.MaxFileSizeBytes)
                throw new ArgumentException($"File size exceeds the allowed limit of {_options.MaxFileSizeBytes} bytes.");

            var extension = NormalizeExtension(Path.GetExtension(file.FileName));
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentException("File extension is required.");

            if (DangerousExtensions.Contains(extension))
                throw new ArgumentException("This file type is not allowed.");

            var allowedExtensions = NormalizeExtensions(_options.AllowedExtensions);
            if (allowedExtensions.Count > 0 && !allowedExtensions.Contains(extension))
                throw new ArgumentException("File extension is not allowed.");

            var contentType = (file.ContentType ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type is required.");

            var allowedContentTypes = NormalizeContentTypes(_options.AllowedContentTypes);
            if (allowedContentTypes.Count > 0 && !allowedContentTypes.Contains(contentType))
                throw new ArgumentException("Content type is not allowed.");
        }

        private static async Task ValidateFileSignatureAsync(Stream fileStream, string extension, CancellationToken ct)
        {
            if (!FileSignatures.TryGetValue(extension, out var signatures) || signatures.Count == 0)
                return;

            var maxSignatureLength = signatures.Max(s => s.Length);
            var header = new byte[maxSignatureLength];
            var readCount = await fileStream.ReadAsync(header.AsMemory(0, maxSignatureLength), ct);
            if (readCount == 0)
                throw new ArgumentException("Uploaded file is empty.");

            var matches = signatures.Any(signature =>
            {
                if (readCount < signature.Length)
                    return false;

                for (var i = 0; i < signature.Length; i++)
                {
                    if (header[i] != signature[i])
                        return false;
                }

                return true;
            });

            if (!matches)
                throw new ArgumentException("File content does not match its extension.");
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return string.Empty;

            var normalized = extension.Trim().ToLowerInvariant();
            return normalized.StartsWith('.') ? normalized : $".{normalized}";
        }

        private static HashSet<string> NormalizeExtensions(IEnumerable<string>? extensions)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (extensions == null)
                return result;

            foreach (var extension in extensions)
            {
                var normalized = NormalizeExtension(extension);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    result.Add(normalized);
                }
            }

            return result;
        }

        private static HashSet<string> NormalizeContentTypes(IEnumerable<string>? contentTypes)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (contentTypes == null)
                return result;

            foreach (var contentType in contentTypes)
            {
                var normalized = (contentType ?? string.Empty).Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    result.Add(normalized);
                }
            }

            return result;
        }

        private static string NormalizeSegment(string value)
        {
            var trimmed = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Invalid storage metadata.");

            var chars = trimmed
                .Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '-')
                .ToArray();

            var normalized = new string(chars).Trim('-');
            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException("Invalid storage metadata.");

            return normalized;
        }
    }
}
