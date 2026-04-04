using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantManagement.Data.Models;
using MultiTenantManagement.Infrastructure.Features.Users.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Users
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserDto>> GetUsersAsync(Guid? tenantIdFilter, CancellationToken ct)
        {
            var q = _userManager.Users.Include(x=>x.Tenant).AsNoTracking().Where(u => !u.IsDeleted); ;

            if (tenantIdFilter.HasValue)
                q = q.Where(u => u.TenantId == tenantIdFilter.Value );

         
            var users = await q.OrderBy(u => u.Email).ToListAsync(ct);

            var result = new List<UserDto>(users.Count);
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    TenantId = u.TenantId,
                    Roles = roles.ToList(),
                    Tenant = u.Tenant?.Name
                });

            }
            return result;
        }

        public async Task<UserDto?> GetByIdAsync(string userId, CancellationToken ct)
        {
            var u = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, ct);
            if (u is null) return null;

            var roles = await _userManager.GetRolesAsync(u);
            return new UserDto{ Id = u.Id, Email = u.Email!,
                PhoneNumber = u.PhoneNumber, TenantId = u.TenantId,
                Roles = roles.ToList() };
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto, string actorUserId, CancellationToken ct)
        {
            var email = dto.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required.");
            if (string.IsNullOrWhiteSpace(dto.Role)) throw new ArgumentException("Role is required.");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = dto.PhoneNumber,
                TenantId = dto.TenantId,
                IsDeleted = false
            };

            var res = await _userManager.CreateAsync(user, dto.Password);
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(", ", res.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, dto.Role);

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                TenantId = user.TenantId,
                Roles = roles.ToList()
            };

        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto req, string actorUserId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted) return false;

            user.PhoneNumber = req.PhoneNumber ?? user.PhoneNumber;

            // TenantId update
            user.TenantId = req.TenantId;

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(", ", res.Errors.Select(e => e.Description)));

            // Role update (optional)
            if (!string.IsNullOrWhiteSpace(req.Role))
            {
                if (!await _roleManager.RoleExistsAsync(req.Role))
                    throw new ArgumentException("Role does not exist.");

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, req.Role);
            }

            return true;
        }

        public async Task<bool> SoftDeleteAsync(string userId, string actorUserId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted) return false;

            user.IsDeleted = true;
            var res = await _userManager.UpdateAsync(user);

            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(", ", res.Errors.Select(e => e.Description)));

            return true;
        }


    }
}
