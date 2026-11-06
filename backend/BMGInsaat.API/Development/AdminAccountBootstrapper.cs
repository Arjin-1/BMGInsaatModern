using BMGInsaat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BMGInsaat.API.Development;

public static class AdminAccountBootstrapper
{
    public static async Task CreateAsync(WebApplication app)
    {
        // Yalnizca gelistirme ortaminda calisabilir
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Admin hesap kurulumu yalnizca Development ortaminda yapilabilir.");
        }

        // Bilgileri User Secrets uzerinden al
        var email = app.Configuration["AdminBootstrap:Email"]?.Trim();
        var password = app.Configuration["AdminBootstrap:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Admin kurulum bilgileri eksik.");
        }

        using var scope = app.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("SuperAdmin"))
        {
            throw new InvalidOperationException(
                "SuperAdmin rolu bulunamadi.");
        }

        // Ayni e-posta ile ikinci hesap olusturulmasin
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "Bu e-posta adresiyle zaten bir hesap mevcut.");
        }

        // Sifre Identity tarafindan guvenli sekilde hashlenir
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(
            user,
            password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Admin hesabi olusturulamadi: " +
                string.Join(", ",
                    createResult.Errors.Select(e => e.Description)));
        }

        var roleResult = await userManager.AddToRoleAsync(
            user,
            "SuperAdmin");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            throw new InvalidOperationException(
                "SuperAdmin rolu atanamadi.");
        }
    }
}
