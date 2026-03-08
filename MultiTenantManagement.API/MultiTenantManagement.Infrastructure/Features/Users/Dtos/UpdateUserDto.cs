using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Users.Dtos
{
    public class UpdateUserDto
    {
        public string? PhoneNumber { get; set; }

        public Guid? TenantId { get; set; }

        public string? Role { get; set; }
    }
}
