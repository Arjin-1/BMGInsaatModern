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
            certificate.ImageUrl,
            certificate.DisplayOrder
        });

        return Ok(result);
    }
}
