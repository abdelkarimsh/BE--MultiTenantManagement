using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Users.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public Guid? TenantId { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
