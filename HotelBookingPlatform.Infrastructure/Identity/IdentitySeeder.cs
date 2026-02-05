using HotelBookingPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBookingPlatform.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<LocalUser>>();

            // Roles à garantir
            var roles = new[] { "Admin", "Staff", "HR", "Manager", "User"};

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed premier Admin uniquement si aucun Admin n'existe
            var existingAdmins = await userManager.GetUsersInRoleAsync("Admin");
            if (existingAdmins != null && existingAdmins.Count > 0) return;

            var adminEmail = config["SeedAdmin:Email"];
            var adminPassword = config["SeedAdmin:Password"];
            var adminFirstName = config["SeedAdmin:FirstName"];
            var adminLastName = config["SeedAdmin:LastName"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                return;

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin != null) return;

            admin = new LocalUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = adminFirstName ?? "System",
                LastName = adminLastName ?? "Admin"
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded) return;

            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
