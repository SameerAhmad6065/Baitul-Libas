using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TailorBooking.Models;

namespace TailorBooking.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        RoleManager<IdentityRole> roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Applies pending migrations in the development project.
        await dbContext.Database.MigrateAsync();

        await CreateRoleAsync(roleManager, RoleNames.Admin);
        await CreateRoleAsync(roleManager, RoleNames.Customer);

        string adminEmail =
            configuration["AdminUser:Email"]
            ?? throw new InvalidOperationException(
                "AdminUser:Email is missing from configuration.");

        string adminPassword =
            configuration["AdminUser:Password"]
            ?? throw new InvalidOperationException(
                "AdminUser:Password is missing from configuration.");

        string adminFullName =
            configuration["AdminUser:FullName"]
            ?? "Website Administrator";

        string? adminWhatsApp =
            configuration["AdminUser:WhatsAppNumber"];

        ApplicationUser? adminUser =
            await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminFullName,
                WhatsAppNumber = adminWhatsApp,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            IdentityResult createResult =
                await userManager.CreateAsync(adminUser, adminPassword);

            if (!createResult.Succeeded)
            {
                string errors = string.Join(
                    Environment.NewLine,
                    createResult.Errors.Select(error =>
                        $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException(
                    $"Admin account creation failed:{Environment.NewLine}{errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                adminUser,
                RoleNames.Admin))
        {
            IdentityResult roleResult =
                await userManager.AddToRoleAsync(
                    adminUser,
                    RoleNames.Admin);

            if (!roleResult.Succeeded)
            {
                string errors = string.Join(
                    Environment.NewLine,
                    roleResult.Errors.Select(error =>
                        $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException(
                    $"Assigning the Admin role failed:{Environment.NewLine}{errors}");
            }
        }
    }

    private static async Task CreateRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        IdentityResult result =
            await roleManager.CreateAsync(
                new IdentityRole(roleName));

        if (!result.Succeeded)
        {
            string errors = string.Join(
                Environment.NewLine,
                result.Errors.Select(error =>
                    $"{error.Code}: {error.Description}"));

            throw new InvalidOperationException(
                $"Creating role '{roleName}' failed:{Environment.NewLine}{errors}");
        }
    }
}