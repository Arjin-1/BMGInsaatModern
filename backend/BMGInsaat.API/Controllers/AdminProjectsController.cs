using BMGInsaat.Application.DTOs.Projects;
using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/projects")]
[Authorize(Roles = "SuperAdmin")]
public class AdminProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public AdminProjectsController(
        IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var projectId = await _projectService.CreateAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            id = projectId,
            message = "Proje basariyla olusturuldu."
        });
    }


[HttpPut("{id:int}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateProjectRequest request,
    CancellationToken cancellationToken)
{
    if (id <= 0)
    {
        return BadRequest(new
        {
            message = "Gecersiz proje ID."
        });
    }

    var updated = await _projectService.UpdateAsync(
        id,
        request,
        cancellationToken);

    if (!updated)
    {
        return NotFound(new
        {
            message = "Proje bulunamadi."
        });
    }

    return Ok(new
    {
        id,
        message = "Proje basariyla guncellendi."
    });
}


[HttpDelete("{id:int}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(
    int id,
    CancellationToken cancellationToken)
{
    if (id <= 0)
    {
        return BadRequest(new
        {
            message = "Gecersiz proje ID."
        });
    }

    var deleted = await _projectService.DeleteAsync(
        id,
        cancellationToken);

    if (!deleted)
    {
        return NotFound(new
        {
            message = "Proje bulunamadi."
        });
    }

    return Ok(new
    {
        id,
        message = "Proje basariyla silindi."
    });
}


}
