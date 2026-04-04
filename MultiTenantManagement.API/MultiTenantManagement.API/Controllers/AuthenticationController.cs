using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultiTenantManagement.Infrastructure.Features.Authentication;
using MultiTenantManagement.Infrastructure.Features.Authentication.Dtos;
using MultiTenantManagement.Infrastructure.Features.Tenant;
using MultiTenantManagement.Infrastructure.Features.Tenant.Dtos;
using System.Security.Claims;

namespace MultiTenantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ITenantService _tenantService;

        public AuthenticationController(IAuthenticationService authenticationService, ITenantService tenantService)
        {
            _authenticationService = authenticationService;
            _tenantService = tenantService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authenticationService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<TenantDto>> GetUserTenant(CancellationToken ct)
        {
            var tenant = await _tenantService.GetUserTenantAsync(ct);

            if (tenant is null)
                return NotFound("Current tenant was not found.");

            return Ok(tenant);
        }

    }
}
