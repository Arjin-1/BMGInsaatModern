using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateRepository _certificateRepository;

    public CertificatesController(
        ICertificateRepository certificateRepository)
    {
        _certificateRepository = certificateRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublishedCertificates(
        CancellationToken cancellationToken)
    {
        var certificates = await _certificateRepository
            .GetPublishedAsync(cancellationToken);

        var result = certificates.Select(certificate => new
        {
            certificate.Id,
            certificate.NameTr,
            certificate.NameEn,
            ImageUrl = string.IsNullOrWhiteSpace(certificate.ImageUrl)
                ? string.Empty
                : $"/api/certificates/{certificate.Id}/image",
            certificate.DisplayOrder
        });

        return Ok(result);
    }

    
    // Sertifika gorselini yayin durumuna gore goster.
    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetCertificateImage(
        int id,
        [FromServices] ICertificateImageStorage imageStorage,
        CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (certificate is null ||
            string.IsNullOrWhiteSpace(certificate.ImageUrl))
        {
            return NotFound();
        }

        // Yayinlanmamis sertifika gorselini
        // yalnizca SuperAdmin gorebilir.
        if (!certificate.IsPublished &&
            !User.IsInRole("SuperAdmin"))
        {
            return NotFound();
        }

        var contentType = Path.GetExtension(certificate.ImageUrl)
            .ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };

        if (contentType is null)
        {
            return NotFound();
        }

        try
        {
            var stream = imageStorage.OpenRead(certificate.ImageUrl);

            if (stream is null)
            {
                return NotFound();
            }

            Response.Headers.CacheControl = "no-store";

            return File(stream, contentType);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

}
