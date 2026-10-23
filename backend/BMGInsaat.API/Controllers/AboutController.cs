using BMGInsaat.Application.DTOs.About;
using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/about")]
[AllowAnonymous]
public class AboutController : ControllerBase
{
    private readonly IAboutService _aboutService;

    public AboutController(IAboutService aboutService)
    {
        _aboutService = aboutService;
    }

    [HttpGet]
    public async Task<ActionResult<AboutContentResponse>> Get(
        CancellationToken cancellationToken)
    {
        var content = await _aboutService.GetAsync(
            cancellationToken);

        if (content is null)
        {
            return NotFound(new
            {
                message = "Hakkimizda icerigi bulunamadi."
            });
        }

        var response = new AboutContentResponse
        {
            AboutTr = content.AboutTr,
            AboutEn = content.AboutEn,
            MissionTr = content.MissionTr,
            MissionEn = content.MissionEn,
            VisionTr = content.VisionTr,
            VisionEn = content.VisionEn
        };

        return Ok(response);
    }
}
