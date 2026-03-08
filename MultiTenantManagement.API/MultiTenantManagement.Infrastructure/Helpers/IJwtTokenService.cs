using MultiTenantManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Helpers
{
    public interface IJwtTokenService
    {
        Task<(string token, DateTime expiresAt)> CreateTokenAsync(ApplicationUser user);
    }
}
