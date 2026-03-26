using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantManagement.Infrastructure.Features.Attachment;
using MultiTenantManagement.Infrastructure.Features.Attachment.Dtos;

namespace MultiTenantManagement.API.Controllers
{
    [Route("api/tenants/{tenantId}/Attachments")]
    [Authorize(Policy = "TenantAccess", Roles = "SystemAdmin,TenantAdmin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentsController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] AttachmentUploadRequestDto request, CancellationToken ct = default)
        {
            try
            {
                var result = await _attachmentService.UploadAsync(request, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            var attachment = await _attachmentService.GetByIdAsync(id, ct);
            if (attachment is null)
                return NotFound();

            return Ok(attachment);
        }
    }
}
