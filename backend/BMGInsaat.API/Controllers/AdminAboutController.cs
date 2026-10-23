using BMGInsaat.Application.DTOs.About;
using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/about")]
[Authorize(Roles = "SuperAdmin")]
public class AdminAboutController : ControllerBase
{
    private readonly IAboutService _aboutService;

    public AdminAboutController(IAboutService aboutService)
    {
        _aboutService = aboutService;
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [FromBody] UpdateAboutContentRequest request,
        CancellationToken cancellationToken)
    {
        await _aboutService.SaveAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            message = "Hakkimizda icerigi basariyla kaydedildi."
        });
    }
}
