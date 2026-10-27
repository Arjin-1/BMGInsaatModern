
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Infrastructure.Identity;
using BMGInsaat.Infrastructure.Persistence;
using BMGInsaat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BMGInsaat.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
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

        // Proje fotograflarini sunucuda saklama
        services.AddScoped<IProjectImageStorage>(provider =>
            new LocalProjectImageStorage(
                provider.GetRequiredService<IHostEnvironment>().ContentRootPath));

        services.AddHttpClient<ITranslationService, AzureTranslationService>();

        // Identity sistemi
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            options.User.RequireUniqueEmail = false;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
