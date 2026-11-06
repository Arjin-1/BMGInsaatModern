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
            ImageUrl = string.IsNullOrWhiteSpace(service.ImageUrl)
                ? string.Empty
                : $"/api/services/{service.Id}/image",
            service.DisplayOrder
        });

        return Ok(result);
    }


    // Hizmet gorselini yayin durumuna gore goster.
    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetServiceImage(
        int id,
        [FromServices] IConstructionServiceImageStorage imageStorage,
        CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (service is null ||
            string.IsNullOrWhiteSpace(service.ImageUrl))
        {
            return NotFound();
        }

        // Yayinlanmamis hizmet gorsellerini
        // yalnizca SuperAdmin gorebilir.
        if (!service.IsPublished &&
            !User.IsInRole("SuperAdmin"))
        {
            return NotFound();
        }

        var contentType = Path.GetExtension(service.ImageUrl)
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
            var stream = imageStorage.OpenRead(service.ImageUrl);

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
