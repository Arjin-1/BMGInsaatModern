using System.ComponentModel.DataAnnotations;
using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/translations")]
[Authorize(Roles = "SuperAdmin")]
public class AdminTranslationsController : ControllerBase
{
    private readonly ITranslationService _translationService;

    public AdminTranslationsController(
        ITranslationService translationService)
    {
        _translationService = translationService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Translate(
        [FromBody] TranslateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new
            {
                message = "Cevrilecek metin bos olamaz."
            });
        }

        var translatedText =
            await _translationService.TranslateToEnglishAsync(
                request.Text,
                cancellationToken);

        return Ok(new
        {
            translatedText
        });
    }
}

public class TranslateRequest
{
    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;
}
