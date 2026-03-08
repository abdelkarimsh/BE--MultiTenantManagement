using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await SeedRolesAsync(roleManager);
            await SeedSystemAdminAsync(userManager, roleManager, config);
        }

        //  ROLES 
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
            "SystemAdmin",
            "TenantAdmin",
            "User"
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Failed to create role '{role}': " +
                            string.Join(", ", result.Errors.Select(e => e.Description))
                        );
                    }
                }
            }
        }

        //  SYSTEM ADMIN
        private static async Task SeedSystemAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            var email = config["Seed:SystemAdmin:Email"];
            var password = config["Seed:SystemAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new Exception("SystemAdmin seed credentials are missing.");

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    TenantId = null,  
                    IsDeleted = false
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to create SystemAdmin: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description))
                    );
                }
            }
            else
            {
                if (user.IsDeleted)
                {
                    user.IsDeleted = false;
                    await userManager.UpdateAsync(user);
                }
            }

            if (!await userManager.IsInRoleAsync(user, "SystemAdmin"))
            {
                await userManager.AddToRoleAsync(user, "SystemAdmin");
            }
        }
    }
}
