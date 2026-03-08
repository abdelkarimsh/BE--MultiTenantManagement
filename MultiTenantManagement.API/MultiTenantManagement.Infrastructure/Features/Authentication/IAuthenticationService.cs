using MultiTenantManagement.Infrastructure.Features.Authentication.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Authentication
{
    public interface IAuthenticationService
    {
        Task<LoginResultDto> LoginAsync(LoginDto dto);
    }
}
