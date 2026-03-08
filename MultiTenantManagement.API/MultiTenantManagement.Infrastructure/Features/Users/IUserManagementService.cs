using MultiTenantManagement.Infrastructure.Features.Users.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Users
{
    public interface IUserManagementService
    {
        Task<List<UserDto>> GetUsersAsync(Guid? tenantIdFilter, CancellationToken ct);
        Task<UserDto?> GetByIdAsync(string userId, CancellationToken ct);

        Task<UserDto> CreateUserAsync(CreateUserDto dto, string actorUserId, CancellationToken ct);
        Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto, string actorUserId, CancellationToken ct);

        Task<bool> SoftDeleteAsync(string userId, string actorUserId, CancellationToken ct);

    }
}
