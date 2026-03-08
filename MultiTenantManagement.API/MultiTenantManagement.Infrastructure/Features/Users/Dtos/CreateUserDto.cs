using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Users.Dtos
{
    public class CreateUserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        public string? PhoneNumber { get; set; }

        public Guid? TenantId { get; set; }

        [Required]
        public string Role { get; set; } = "User";
    }
}
