using Microsoft.AspNetCore.Mvc;
using MultiTenantManagement.Infrastructure.Features.Tenant.Dtos;
using MultiTenantManagement.Infrastructure.Features.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using MultiTenantManagement.Infrastructure.Helpers;

namespace MultiTenantManagement.API.Controllers
{
    [ApiController]
    [Authorize(Policy = "SuperAdminOnly", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
            => _tenantService = tenantService;

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


        [HttpGet]
        [Route("Tenant/{id}")]
        public async Task<ActionResult<TenantDto>> GetById([FromRoute]Guid id, CancellationToken ct)
        {
            var tenant = await _tenantService.GetByIdAsync(id, ct);
            return tenant is null ? NotFound() : Ok(tenant);
        }

        [HttpPost]
        [Route("CreateTenant")]
        public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequestDto req, CancellationToken ct)
            => Ok(await _tenantService.CreateAsync(req, ct));

        [HttpPut]
        [Route("UpdateTenant")]
        public async Task<IActionResult> Update([FromBody] UpdateTenantRequestDto req, CancellationToken ct)
        {
            var ok = await _tenantService.UpdateAsync( req, ct);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete]
        [Route("Tenant/{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var ok = await _tenantService.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
