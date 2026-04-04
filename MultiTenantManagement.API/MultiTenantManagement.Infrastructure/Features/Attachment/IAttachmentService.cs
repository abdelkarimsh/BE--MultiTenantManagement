using MultiTenantManagement.Infrastructure.Features.Attachment.Dtos;

namespace MultiTenantManagement.Infrastructure.Features.Attachment
{
    public interface IAttachmentService
    {
        Task<AttachmentUploadResultDto> UploadAsync(Guid tenantId ,AttachmentUploadRequestDto request, CancellationToken ct = default);
        Task<AttachmentDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    }
}
