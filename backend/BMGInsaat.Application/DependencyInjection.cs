using BMGInsaat.Application.Interfaces;
using BMGInsaat.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BMGInsaat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAboutService, AboutService>();
        services.AddScoped<ICompanyValueService, CompanyValueService>();


        return services;
    }
}
