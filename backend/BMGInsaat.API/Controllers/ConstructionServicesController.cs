using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/services")]
public class ConstructionServicesController : ControllerBase
{
    private readonly IConstructionServiceRepository _serviceRepository;

    public ConstructionServicesController(
        IConstructionServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    // Ziyaretciler: Yalnizca yayinlanmis hizmetleri listele
    [HttpGet]
    public async Task<IActionResult> GetPublishedServices(
        CancellationToken cancellationToken)
    {
        var services = await _serviceRepository
            .GetPublishedAsync(cancellationToken);

        var result = services.Select(service => new
        {
            service.Id,
            service.TitleTr,
            service.TitleEn,
            service.DescriptionTr,
            service.DescriptionEn,
            service.ImageUrl,
            service.DisplayOrder
        });

        return Ok(result);
    }
}
