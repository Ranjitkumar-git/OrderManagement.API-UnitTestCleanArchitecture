using Microsoft.AspNetCore.Identity;
using OrderManagement.API.Constants;
using OrderManagement.API.Models.Identity;

namespace OrderManagement.API.Data.Seed
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@ordermanagement.com";

            var existingAdmin =
                await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin != null)
                return;

            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FirstName = "System",
                LastName = "Administrator",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");

            if (!result.Succeeded)
            {
                Console.WriteLine("Admin creation failed.");

                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"{error.Code} - {error.Description}");
                }

                return;
            }

            Console.WriteLine("Admin created successfully.");

            var roleResult = await userManager.AddToRoleAsync(admin, Roles.Admin);

            if (!roleResult.Succeeded)
            {
                Console.WriteLine("Role assignment failed.");

                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine($"{error.Code} - {error.Description}");
                }
            }
            else
            {
                Console.WriteLine("Admin role assigned successfully.");
            }
        }
    }
}