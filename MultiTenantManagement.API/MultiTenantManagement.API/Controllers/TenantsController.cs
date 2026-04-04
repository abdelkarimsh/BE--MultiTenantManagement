using Microsoft.AspNetCore.Mvc;
using MultiTenantManagement.Infrastructure.Features.Tenant.Dtos;
using MultiTenantManagement.Infrastructure.Features.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using MultiTenantManagement.Infrastructure.Helpers;
using MultiTenantManagement.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MultiTenantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
            => _tenantService = tenantService;

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpGet]
        [Route("GetTenants")]
        public async Task<ActionResult<PagedResult<TenantDto>>> GetTenants(
             [FromQuery] int pageNumber = 1,
             [FromQuery] int pageSize = 10,
             [FromQuery] string? sortBy = "createdAtUtc",
             [FromQuery] bool isAscending = false,
             [FromQuery] string? search = null,
             [FromQuery] bool? isActive = null,
            CancellationToken ct = default)
        {
            var result = await _tenantService.GetPagedAsync(
                pageNumber,
                pageSize,
                sortBy,
                isAscending,
                search,
                isActive,
                ct);

            return Ok(result);
        }

        [Authorize(Policy = "TenantAccess", Roles = "SystemAdmin,TenantAdmin")]
        [HttpGet]
        [Route("Tenant/{tenantId}")]
        public async Task<ActionResult<TenantDto>> GetById([FromRoute]Guid tenantId, CancellationToken ct)
        {
            var tenant = await _tenantService.GetByIdAsync(tenantId, ct);
            return tenant is null ? NotFound() : Ok(tenant);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost]
        [Route("CreateTenant")]
        public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequestDto req, CancellationToken ct)
            => Ok(await _tenantService.CreateAsync(req, ct));

        [Authorize(Policy = "TenantAccess", Roles = "SystemAdmin,TenantAdmin")]
        [HttpPut]
        [Route("UpdateTenant/{tenantId}")]
        public async Task<IActionResult> Update([FromRoute] Guid tenantId ,[FromBody] UpdateTenantRequestDto req, CancellationToken ct)
        {
            if (req.Id != tenantId)
                return BadRequest("Route tenantId does not match request Id.");
            var ok = await _tenantService.UpdateAsync( req, ct);
            return ok ? NoContent() : NotFound();
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpDelete]
        [Route("Tenant/{tenantId}")]
        public async Task<IActionResult> Delete([FromRoute]Guid tenantId, CancellationToken ct)
        {
            var ok = await _tenantService.DeleteAsync(tenantId, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
