using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace BMGInsaat.Infrastructure.Services;

public sealed class OutlookTokenService
{
    private static readonly string[] Scopes = ["Mail.Send"];

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public OutlookTokenService(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    // Outlook hesabini ilk kez yetkilendir.
    // Yalnizca gelistirme ortaminda kullanilir.
    public async Task AuthorizeForDevelopmentAsync(
        Action<string> showInstructions,
        CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Ilk Outlook yetkilendirmesi yalnizca Development ortaminda yapilabilir.");
        }

        var app = await CreateClientAsync();

        await app.AcquireTokenWithDeviceCode(
                Scopes,
                deviceCode =>
                {
                    showInstructions(deviceCode.Message);
                    return Task.CompletedTask;
                })
            .ExecuteAsync(cancellationToken);
    }

    // Daha once yetkilendirilen hesaptan token al.
    // API istekleri sirasinda kullaniciya giris ekrani acilmaz.
    public async Task<string> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
    {
        var app = await CreateClientAsync();

        var accounts = (await app.GetAccountsAsync()).ToList();

        if (accounts.Count != 1)
        {
            throw new InvalidOperationException(
                "Outlook icin tam olarak bir yetkilendirilmis hesap bulunmalidir.");
        }

        try
        {
            var result = await app
                .AcquireTokenSilent(Scopes, accounts[0])
                .ExecuteAsync(cancellationToken);

            return result.AccessToken;
        }
        catch (MsalUiRequiredException ex)
        {
            throw new InvalidOperationException(
                "Outlook hesabi yeniden yetkilendirilmeli.",
                ex);
        }
    }

    // MSAL uygulamasini ve korumali token onbellegini hazirla.
    private async Task<IPublicClientApplication> CreateClientAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "Bu yerel token saklama yapisi Windows icin hazirlanmistir.");
        }

        var clientId = _configuration["AdminEmail:ClientId"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException(
                "Outlook Client ID bulunamadi.");
        }

        var cacheDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "BMGInsaat",
            "Auth");

        Directory.CreateDirectory(cacheDirectory);

        var storageProperties =
            new StorageCreationPropertiesBuilder(
                "outlook-token-cache.bin",
                cacheDirectory)
            .Build();

        var app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(
                "https://login.microsoftonline.com/consumers")
            .WithRedirectUri(
                "https://login.microsoftonline.com/common/oauth2/nativeclient")
            .Build();

        var cacheHelper = await MsalCacheHelper.CreateAsync(
            storageProperties);

        cacheHelper.RegisterCache(app.UserTokenCache);

        return app;
    }
}
