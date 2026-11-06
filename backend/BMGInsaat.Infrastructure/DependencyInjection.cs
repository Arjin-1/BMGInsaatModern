using BMGInsaat.Application.Interfaces;
using BMGInsaat.Infrastructure.Identity;
using BMGInsaat.Infrastructure.Persistence;
using BMGInsaat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BMGInsaat.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
namespace BMGInsaat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Veritabani baglanti bilgisi bulunamadi.");
        }

        // PostgreSQL baglantisi
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repository kaydi
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAboutRepository, AboutRepository>();
        services.AddScoped<ICompanyValueRepository, CompanyValueRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IConstructionServiceRepository, ConstructionServiceRepository>();
        services.AddScoped<IProjectVideoRepository, ProjectVideoRepository>();


        // Gelistirmede mevcut klasoru kullan.
        // Production ortaminda kalici depolama yolu zorunlu.
        string GetStorageBasePath(IServiceProvider provider)
        {
            var environment = provider
                .GetRequiredService<IHostEnvironment>();

            var configuredPath =
                configuration["Storage:BasePath"]?.Trim();

            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                if (!environment.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        "Production icin Storage:BasePath ayari zorunludur.");
                }

                return environment.ContentRootPath;
            }

            if (!Path.IsPathFullyQualified(configuredPath))
            {
                throw new InvalidOperationException(
                    "Storage:BasePath mutlak bir klasor yolu olmalidir.");
            }


            var storagePath = Path.GetFullPath(configuredPath);

            // Production depolamasi uygulamanin yayin klasoru disinda olmali.
            if (!environment.IsDevelopment())
            {
                var relativePath = Path.GetRelativePath(
                    environment.ContentRootPath,
                    storagePath);

                var isOutsideApplication =
                    Path.IsPathRooted(relativePath) ||
                    relativePath == ".." ||
                    relativePath.StartsWith(
                        ".." + Path.DirectorySeparatorChar,
                        StringComparison.Ordinal);

                if (!isOutsideApplication)
                {
                    throw new InvalidOperationException(
                        "Production depolama klasoru uygulama klasorunun disinda olmalidir.");
                }
            }

            return storagePath;

        }

        // Proje fotograflari
        services.AddScoped<IProjectImageStorage>(provider =>
            new LocalProjectImageStorage(
                GetStorageBasePath(provider)));

        // Sertifika gorselleri
        services.AddScoped<ICertificateImageStorage>(provider =>
            new LocalCertificateImageStorage(
                GetStorageBasePath(provider)));

        // Hizmet gorselleri
        services.AddScoped<IConstructionServiceImageStorage>(provider =>
            new LocalConstructionServiceImageStorage(
                GetStorageBasePath(provider)));

        // Proje videolari
        services.AddScoped<IProjectVideoStorage>(provider =>
            new LocalProjectVideoStorage(
                GetStorageBasePath(provider)));


        services.AddHttpClient<ITranslationService, AzureTranslationService>();

        // Admin e-posta dogrulama ve sifre sifirlama servisi

        
services.AddScoped<IAdminAccountEmailService>(provider =>
{
    var environment = provider
        .GetRequiredService<IHostEnvironment>();

    if (environment.IsDevelopment())
    {
        return ActivatorUtilities
            .CreateInstance<GraphAdminAccountEmailService>(provider);
    }

    return ActivatorUtilities
        .CreateInstance<SmtpAdminAccountEmailService>(provider);
});



        // Microsoft Graph OAuth2 token yonetimi
        services.AddSingleton<OutlookTokenService>();



        // Identity sistemi
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();


        // Identity tokenlarinin gecerlilik suresi
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(15);
        });


        return services;
    }
}
