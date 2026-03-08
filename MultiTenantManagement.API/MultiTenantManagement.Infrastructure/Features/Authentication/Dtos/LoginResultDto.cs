using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Authentication.Dtos
{
    public class LoginResultDto
    {
        public string AccessToken { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public string Email { get; set; }
        public string? UserRole { get; set; }
        public Guid? TenantId { get; set; }
        public string? FullName { get; set; }
    }
}
