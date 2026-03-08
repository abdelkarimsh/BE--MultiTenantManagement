using Microsoft.AspNetCore.Identity;
using MultiTenantManagement.Data.Models;
using MultiTenantManagement.Infrastructure.Features.Authentication.Dtos;
using MultiTenantManagement.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                throw new UnauthorizedAccessException("Invalid email or password.");


            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                dto.Password,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);

            var role = roles.FirstOrDefault();

            var (token, expiresAt) = await _jwtTokenService.CreateTokenAsync(user);
            return new LoginResultDto
            {
                AccessToken = token,
                ExpiresAtUtc = expiresAt,
                Email = user.Email,
                FullName = user.FullName,
                TenantId = user.TenantId,
                UserRole = role
            };
        }
    }
}
