using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;

    public ProjectsController(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublishedProjects(
        CancellationToken cancellationToken)
    {
        var projects = await _projectRepository
            .GetPublishedAsync(cancellationToken);

        var result = projects.Select(project => new
        {
            project.Id,
            project.NameTr,
            project.NameEn,
            project.DescriptionTr,
            project.DescriptionEn,
            project.City,
            project.EmployerTr,
            project.EmployerEn,
            project.StartDate,
            project.EndDate,
            project.IsFeatured,
            project.Status,

            Images = project.Images
                .OrderBy(image => image.DisplayOrder)
                .Select(image => new
                {
                    image.Id,
                    ImageUrl = $"/api/projects/{project.Id}/images/{image.Id}",
                    image.IsCover,
                    image.DisplayOrder
                })
                .ToList()
        });

        return Ok(result);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProjectById(
        int id,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository
            .GetByIdAsync(id, cancellationToken);

        // Yayınlanmamış veya bulunamayan projeler gösterilmez.
        if (project is null || !project.IsPublished)
        {
            return NotFound();
        }

        var result = new
        {
            project.Id,
            project.NameTr,
            project.NameEn,
            project.DescriptionTr,
            project.DescriptionEn,
            project.City,
            project.EmployerTr,
            project.EmployerEn,
            project.StartDate,
            project.EndDate,
            project.IsFeatured,
            project.Status,

            Images = project.Images
                .OrderBy(image => image.DisplayOrder)
                .Select(image => new
                {
                    image.Id,
                    ImageUrl = $"/api/projects/{project.Id}/images/{image.Id}",
                    image.IsCover,
                    image.DisplayOrder
                })
                .ToList()
        };

        return Ok(result);
    }

    
    // Proje fotograflarini yayin durumuna gore goster.
    [HttpGet("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> GetProjectImage(
        int id,
        int imageId,
        [FromServices] IProjectImageStorage imageStorage,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        // Yayinlanmamis projelerin fotograflarini
        // yalnizca SuperAdmin gorebilir.
        if (!project.IsPublished &&
            !User.IsInRole("SuperAdmin"))
        {
            return NotFound();
        }

        var image = project.Images
            .FirstOrDefault(x => x.Id == imageId);

        if (image is null)
        {
            return NotFound();
        }

        var contentType = Path.GetExtension(image.ImageUrl)
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
            var stream = imageStorage.OpenRead(image.ImageUrl);

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
