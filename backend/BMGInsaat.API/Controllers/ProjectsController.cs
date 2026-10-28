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
                    image.ImageUrl,
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
                    image.ImageUrl,
                    image.IsCover,
                    image.DisplayOrder
                })
                .ToList()
        };

        return Ok(result);
    }

}
