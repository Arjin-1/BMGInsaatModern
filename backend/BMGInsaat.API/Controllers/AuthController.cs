using BMGInsaat.API.Contracts.Auth;
using BMGInsaat.Infrastructure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;


namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAntiforgery _antiforgery;
    private readonly IAdminAccountEmailService _adminEmailService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAntiforgery antiforgery,
        IAdminAccountEmailService adminEmailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _antiforgery = antiforgery;
        _adminEmailService = adminEmailService;
    }

    [HttpGet("csrf")]
    [AllowAnonymous]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            csrfToken = tokens.RequestToken
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {

        var user = await _userManager.FindByEmailAsync(
            request.Email.Trim());


        if (user is null ||
            !await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            return Unauthorized(new
            {
                message = "E-posta veya sifre hatali."
            });
        }

        // Giris icin yonetici hesabinin
        // e-posta adresi dogrulanmis olmalidir.
        if (!string.IsNullOrWhiteSpace(user.Email) &&
            !await _userManager.IsEmailConfirmedAsync(user))
        {
            return Unauthorized(new
            {
                message = "E-posta veya sifre hatali."
            });
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                message = "Giris basarisiz. Bilgilerinizi kontrol edin."
            });
        }

        return Ok(new
        {
            message = "Giris basarili.",
            email = user.Email,
            role = "SuperAdmin"
        });
    }


    // Admin e-posta adresini Identity tokeni ile dogrula
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailRequest request)
    {
        var userId = request.UserId?.Trim();
        var token = request.Token?.Trim();

        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new
            {
                message = "Dogrulama baglantisi gecersiz veya suresi dolmus."
            });
        }

        // Dogrulanacak kullaniciyi bul
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null ||
            !await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            return BadRequest(new
            {
                message = "Dogrulama baglantisi gecersiz veya suresi dolmus."
            });
        }

        // ASP.NET Core Identity tokenini dogrula
        var result = await _userManager.ConfirmEmailAsync(
            user,
            token);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Dogrulama baglantisi gecersiz veya suresi dolmus."
            });
        }

        return Ok(new
        {
            message = "E-posta adresiniz basariyla dogrulandi."
        });
    }


    // Sifremi Unuttum - Sifre sifirlama baglantisi gonder
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] ILogger<AuthController> logger,
        [FromServices] IConfiguration configuration,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        // Hesabin var olup olmadigini aciklamayan ortak yanit
        const string responseMessage =
            "Eger bu e-posta adresine ait uygun bir hesap varsa, " +
            "sifre sifirlama baglantisi gonderilecektir.";

        var email = request.Email.Trim();

        // E-posta adresine ait hesabi bul
        var user = await _userManager.FindByEmailAsync(email);

        // Yalnizca dogrulanmis SuperAdmin hesaplari
        // sifre sifirlama e-postasi alabilir.
        if (user is null ||
            !await _userManager.IsInRoleAsync(user, "SuperAdmin") ||
            !await _userManager.IsEmailConfirmedAsync(user))
        {
            return Ok(new
            {
                message = responseMessage
            });
        }


        var token = await _userManager
            .GeneratePasswordResetTokenAsync(user);

        // Tokeni URL icin guvenli formata donustur
        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));


        var frontendBaseUrl = configuration["Frontend:BaseUrl"]?
            .Trim()
            .TrimEnd('/');

        if (string.IsNullOrWhiteSpace(frontendBaseUrl) ||
            !Uri.TryCreate(
                frontendBaseUrl,
                UriKind.Absolute,
                out var frontendUri) ||
            (frontendUri.Scheme != Uri.UriSchemeHttps &&
             !(environment.IsDevelopment() &&
               frontendUri.Scheme == Uri.UriSchemeHttp)))
        {
            throw new InvalidOperationException(
                "Frontend:BaseUrl ayari eksik veya gecersiz.");
        }

        var resetUrl =
            frontendBaseUrl + "/admin/reset-password" +
            "?email=" + Uri.EscapeDataString(user.Email!) +
            "&token=" + encodedToken;


        var message =
            "BMG Insaat Yonetim Sistemi\n\n" +
            "Yonetici hesabinizin sifresini sifirlamak icin " +
            "asagidaki baglantiyi acin:\n\n" +
            resetUrl +
            "\n\nBu islemi siz baslatmadiysaniz " +
            "bu e-postayi dikkate almayin.";

        try
        {
            // Microsoft Graph uzerinden e-posta gonder
            await _adminEmailService.SendAsync(
                user.Email!,
                "BMG Insaat - Sifre Sifirlama",
                message,
                cancellationToken);
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException)
        {
            // E-posta gonderim hatasini sunucuya kaydet.
            // Hesabin varligini istemciye aciklama.
            logger.LogError(
                ex,
                "Sifre sifirlama e-postasi gonderilemedi.");
        }

        return Ok(new
        {
            message = responseMessage
        });
    }


    // Sifre Sifirlama - Yeni sifreyi kaydet
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] ILogger<AuthController> logger)
    {
        // E-posta adresini kontrol et
        var email = request.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        // Yalnizca dogrulanmis SuperAdmin hesabi
        if (user is null ||
            !await _userManager.IsInRoleAsync(user, "SuperAdmin") ||
            !await _userManager.IsEmailConfirmedAsync(user))
        {
            return BadRequest(new
            {
                message = "Sifre sifirlama istegi gecersiz veya suresi dolmus."
            });
        }


        string decodedToken;

        try
        {
            // URL icin kodlanan tokeni orijinal haline cevir
            decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            return BadRequest(new
            {
                message = "Sifre sifirlama baglantisi gecersiz."
            });
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            decodedToken,
            request.NewPassword);



        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                logger.LogWarning(
                    "Sifre sifirlama Identity hata kodu: {ErrorCode}",
                    error.Code);
            }

            return BadRequest(new
            {
                message =
                    "Sifre sifirlanamadi. Baglanti gecersiz veya " +
                    "suresi dolmus olabilir ya da yeni sifre " +
                    "guvenlik kurallarini karsilamiyor olabilir."
            });
        }


        return Ok(new
        {
            message = "Sifreniz basariyla degistirildi. Yeni sifrenizle giris yapabilirsiniz."
        });
    }


    [HttpGet("me")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            authenticated = User.Identity?.IsAuthenticated
        });
    }

    [HttpPost("logout")]
    [Authorize(Roles = "SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return Ok(new
        {
            message = "Cikis basarili."
        });
    }
}
