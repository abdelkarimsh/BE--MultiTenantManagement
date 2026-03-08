using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Auth
{
    public class TenantAccessHandler : AuthorizationHandler<TenantAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantAccessRequirement requirement)
        {
            var httpContext = (context.Resource as DefaultHttpContext) ??
                              (context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)?.HttpContext;

            if (httpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var user = httpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // SystemAdmin bypass
            if (user.IsInRole("SystemAdmin") ||
                user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "SystemAdmin") ||
                user.Claims.Any(c => c.Type == "role" && c.Value == "SystemAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Get tenantId from route
            var routeTenantId = httpContext.Request.RouteValues.TryGetValue("tenantId", out var routeTenantIdObj)
                ? routeTenantIdObj?.ToString()
                : null;

            // Get tenantId from claims
            var claimTenantId = user.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;

            if (!string.IsNullOrEmpty(routeTenantId) && !string.IsNullOrEmpty(claimTenantId) &&
                string.Equals(routeTenantId, claimTenantId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
