using BMGInsaat.Application.Interfaces;
using BMGInsaat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BMGInsaat.API.Development;

public static class AdminEmailConfirmationSender
{
    public static async Task SendAsync(
        WebApplication app,
        CancellationToken cancellationToken = default)
    {
        // Yalnizca gelistirme ortaminda kullanilabilir
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "E-posta dogrulama kurulumu yalnizca Development ortaminda calisabilir.");
        }

        var email = app.Configuration["AdminEmail:TestRecipient"]?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                "Admin e-posta adresi bulunamadi.");
        }

        using var scope = app.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var emailService = scope.ServiceProvider
            .GetRequiredService<IAdminAccountEmailService>();

        // Mevcut SuperAdmin hesabini bul
        var user = await userManager.FindByEmailAsync(email);

        if (user is null ||
            !await userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            throw new InvalidOperationException(
                "Bu e-posta adresine ait SuperAdmin hesabi bulunamadi.");
        }

        if (await userManager.IsEmailConfirmedAsync(user))
        {
            throw new InvalidOperationException(
                "Bu hesabin e-posta adresi zaten dogrulanmis.");
        }

        // ASP.NET Core Identity dogrulama tokeni olustur
        var token = await userManager
            .GenerateEmailConfirmationTokenAsync(user);


        // Frontend adresini ortam yapilandirmasindan al
        var frontendBaseUrl = app.Configuration["Frontend:BaseUrl"]?
            .Trim()
            .TrimEnd('/');

        if (string.IsNullOrWhiteSpace(frontendBaseUrl) ||
            !Uri.TryCreate(frontendBaseUrl, UriKind.Absolute, out var frontendUri) ||
            (frontendUri.Scheme != Uri.UriSchemeHttps &&
             frontendUri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException(
                "Gecerli bir Frontend:BaseUrl yapilandirmasi bulunamadi.");
        }

        var verificationUrl =
            frontendBaseUrl + "/admin/confirm-email" +
            "?userId=" + Uri.EscapeDataString(user.Id) +
            "&token=" + Uri.EscapeDataString(token);


        var message =
            "BMG Insaat Yonetim Sistemi\n\n" +
            "SuperAdmin e-posta adresinizi dogrulamak icin " +
            "asagidaki baglantiyi acin:\n\n" +
            verificationUrl +
            "\n\nBu islemi siz baslatmadiysaniz mesaji dikkate almayin.";

        // Microsoft Graph uzerinden e-posta gonder
        await emailService.SendAsync(
            email,
            "BMG Insaat - E-posta Dogrulama",
            message,
            cancellationToken);
    }
}
