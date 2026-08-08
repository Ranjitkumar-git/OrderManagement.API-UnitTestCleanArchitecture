using Microsoft.AspNetCore.Identity;

namespace OrderManagement.API.Data.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            Console.WriteLine("===== Role Seeding Started =====");

            string[] roles =
            {
                "Admin",
                "HR",
                "Manager",
                "Employee"
            };

            foreach (var role in roles)
            {
                Console.WriteLine($"Checking Role : {role}");

                if (!await roleManager.RoleExistsAsync(role))
                {
                    Console.WriteLine($"{role} does not exist. Creating...");

                    var result = await roleManager.CreateAsync(
                        new IdentityRole<Guid>
                        {
                            Name = role
                        });

                    if (!result.Succeeded)
                    {
                        Console.WriteLine($"Failed to create role : {role}");

                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"{error.Code} - {error.Description}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Role {role} Created Successfully");
                    }
                }
                else
                {
                    Console.WriteLine($"Role {role} already exists.");
                }
            }

            Console.WriteLine("===== Role Seeding Completed =====");
        }
    }
}