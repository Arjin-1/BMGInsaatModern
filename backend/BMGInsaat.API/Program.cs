using BMGInsaat.Application;
using BMGInsaat.Infrastructure;
using Microsoft.AspNetCore.Identity;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Infrastructure.Services;
using BMGInsaat.API.Development;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;




var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var keysPath = builder.Configuration["DataProtection:KeysPath"];
    var certificatePath = builder.Configuration["DataProtection:CertificatePath"];
    var certificatePassword = builder.Configuration["DataProtection:CertificatePassword"];

    if (string.IsNullOrWhiteSpace(keysPath) ||
        !Path.IsPathFullyQualified(keysPath) ||
        !Directory.Exists(keysPath) ||
        string.IsNullOrWhiteSpace(certificatePath) ||
        !File.Exists(certificatePath) ||
        string.IsNullOrWhiteSpace(certificatePassword))
    {
        throw new InvalidOperationException(
            "Production Data Protection yapilandirmasi eksik.");
    }

    builder.Services.AddDataProtection()
        .SetApplicationName("BMGInsaat.API")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .ProtectKeysWithCertificate(
            X509CertificateLoader.LoadPkcs12FromFile(
                certificatePath,
                certificatePassword));
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
// Oturum güvenlik damgasını her istekte kontrol et.
// Eski oturumları geçersiz kılmak için gereklidir.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BMGInsaat.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;

    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    };
});


builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";

    options.Cookie.Name = "BMGInsaat.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;

    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
});




builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();


// Iletisim formu e-posta gonderim servisi
builder.Services.AddScoped<IContactEmailService, SmtpContactEmailService>();


builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
    
    
    // Admin girisi: IP basina 15 dakikada en fazla 10 istek
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    
    
    // E-posta dogrulama: IP basina 15 dakikada en fazla 10 istek
    options.AddPolicy("confirm-email", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));



    // Sifre sifirlama: IP basina 15 dakikada en fazla 3 istek
    options.AddPolicy("forgot-password", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    
    // Sifre sifirlama: IP basina 15 dakikada en fazla 5 istek
    options.AddPolicy("reset-password", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

});


var app = builder.Build();






if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // Production ortaminda hatalari guvenli sekilde yonet
    app.UseExceptionHandler();

    // HTTPS kullanimini guclendir
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.Run();

