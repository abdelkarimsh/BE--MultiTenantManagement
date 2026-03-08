using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Helpers
{
    public static class ClaimsExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier);

        public static Guid? GetTenantId(this ClaimsPrincipal user)
        {
            var val = user.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(val, out var g) ? g : null;
        }

        public static bool IsTenantAdmin(this ClaimsPrincipal user)
            => user.IsInRole("TenantAdmin");
    }
}
