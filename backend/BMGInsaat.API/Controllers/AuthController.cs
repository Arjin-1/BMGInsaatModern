using BMGInsaat.API.Contracts.Auth;
using BMGInsaat.Infrastructure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAntiforgery _antiforgery;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAntiforgery antiforgery)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _antiforgery = antiforgery;
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
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(
            request.Username);

        if (user is null ||
            !await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            return Unauthorized(new
            {
                message = "Kullanici adi veya sifre hatali."
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
            username = user.UserName,
            role = "SuperAdmin"
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
