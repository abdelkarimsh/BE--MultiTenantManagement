using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MultiTenantManagement.Infrastructure.Features.Attachment.Dtos;

public class AttachmentUploadRequestDto
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;
}
