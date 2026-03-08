using Microsoft.AspNetCore.Http;
using MultiTenantManagement.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Helpers
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TenantIdClaimName = "tenant_id";
        private const string IsSuperAdminClaimName = "is_super_admin";

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public Guid? TenantId
        {
            get
            {
                var claim = User?.FindFirst(TenantIdClaimName);
                if (claim == null)
                    return null;

                return Guid.TryParse(claim.Value, out var tenantId) ? tenantId : null;
            }
        }

        public bool IsSuperAdmin
        {
            get
            {
                var claim = User?.FindFirst(IsSuperAdminClaimName);
                if (claim?.Value == "true")
                    return true;

                return false;
            }
        }
    }
}
