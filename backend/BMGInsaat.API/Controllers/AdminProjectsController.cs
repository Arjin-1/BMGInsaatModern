using BMGInsaat.Application.DTOs.Projects;
using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BMGInsaat.Domain.Entities;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/projects")]
[Authorize(Roles = "SuperAdmin")]
public class AdminProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectImageStorage _projectImageStorage;

    public AdminProjectsController(
        IProjectService projectService,
        IProjectRepository projectRepository,
        IProjectImageStorage projectImageStorage)
    {
        _projectService = projectService;
        _projectRepository = projectRepository;
        _projectImageStorage = projectImageStorage;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetAllAsync(
            cancellationToken);

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

            project.Status,
            project.IsPublished,
            project.IsFeatured,

            project.CreatedAtUtc,
            project.UpdatedAtUtc,

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


    [HttpPost("{id:int}/images")]
    [Authorize(Roles = "SuperAdmin")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadImage(
        int id,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz proje ID."
            });
        }

        var project = await _projectRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (project is null)
        {
            return NotFound(new
            {
                message = "Proje bulunamadi."
            });
        }

        const long maxFileSize = 15_000_000;

        if (file is null || file.Length == 0 ||
            file.Length > maxFileSize)
        {
            return BadRequest(new
            {
                message = "Fotograf bos olamaz ve 15 MB sinirini asamaz."
            });
        }

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        var validExtensions = new[]
        {
        ".jpg", ".jpeg", ".png", ".webp"
    };

        if (!validExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Yalnizca JPG, PNG ve WebP yuklenebilir."
            });
        }

        // Gercek dosya imzasini kontrol et
        var header = new byte[12];

        await using (var input = file.OpenReadStream())
        {
            var bytesRead = await input.ReadAsync(
                header.AsMemory(),
                cancellationToken);

            if (bytesRead < 12)
            {
                return BadRequest(new
                {
                    message = "Gecersiz fotograf dosyasi."
                });
            }
        }

        var isJpeg =
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF;

        var isPng =
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A;

        var isWebp =
            header[0] == (byte)'R' &&
            header[1] == (byte)'I' &&
            header[2] == (byte)'F' &&
            header[3] == (byte)'F' &&
            header[8] == (byte)'W' &&
            header[9] == (byte)'E' &&
            header[10] == (byte)'B' &&
            header[11] == (byte)'P';

        var validSignature = extension switch
        {
            ".jpg" or ".jpeg" => isJpeg,
            ".png" => isPng,
            ".webp" => isWebp,
            _ => false
        };

        if (!validSignature)
        {
            return BadRequest(new
            {
                message = "Dosya icerigi fotograf formatiyla uyusmuyor."
            });
        }

        string imageUrl;

        await using (var stream = file.OpenReadStream())
        {
            imageUrl = await _projectImageStorage.SaveAsync(
                stream,
                extension,
                cancellationToken);
        }

        var image = new ProjectImage
        {
            ProjectId = project.Id,
            ImageUrl = imageUrl,
            IsCover = project.Images.Count == 0,
            DisplayOrder = project.Images.Count,
            CreatedAtUtc = DateTime.UtcNow
        };

        try
        {
            project.Images.Add(image);

            await _projectRepository.UpdateAsync(
                cancellationToken);
        }
        catch
        {
            await _projectImageStorage.DeleteAsync(
                imageUrl,
                CancellationToken.None);

            throw;
        }

        return Ok(new
        {
            image.Id,
            image.ImageUrl,
            image.IsCover,
            image.DisplayOrder,
            message = "Fotograf basariyla yuklendi."
        });
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


    [HttpDelete("{id:int}/images/{imageId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(
        int id,
        int imageId,
        CancellationToken cancellationToken)
    {
        if (id <= 0 || imageId <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz proje veya fotograf ID."
            });
        }

        var project = await _projectRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (project is null)
        {
            return NotFound(new
            {
                message = "Proje bulunamadi."
            });
        }

        var image = project.Images
            .FirstOrDefault(i => i.Id == imageId);

        if (image is null)
        {
            return NotFound(new
            {
                message = "Bu projeye ait fotograf bulunamadi."
            });
        }

        var imageUrl = image.ImageUrl;
        var wasCover = image.IsCover;

        // Yalnizca secilen fotografi projeden kaldir.
        project.Images.Remove(image);

        // Kapak siliniyorsa kalan fotograflardan yeni kapak sec.
        if (wasCover)
        {
            var newCover = project.Images
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.Id)
                .FirstOrDefault();

            if (newCover is not null)
            {
                newCover.IsCover = true;
            }
        }

        // Once veritabanindaki degisiklikleri kaydet.
        await _projectRepository.UpdateAsync(
            cancellationToken);

        // Ardindan fiziksel fotograf dosyasini temizle.
        await _projectImageStorage.DeleteAsync(
            imageUrl,
            cancellationToken);

        return Ok(new
        {
            imageId,
            message = "Fotograf basariyla silindi."
        });
    }

    [HttpPut("{id:int}/images/{imageId:int}/cover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCoverImage(
        int id,
        int imageId,
        CancellationToken cancellationToken)
    {
        if (id <= 0 || imageId <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz proje veya fotograf ID."
            });
        }

        var project = await _projectRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (project is null)
        {
            return NotFound(new
            {
                message = "Proje bulunamadi."
            });
        }

        var selectedImage = project.Images
            .FirstOrDefault(image => image.Id == imageId);

        if (selectedImage is null)
        {
            return NotFound(new
            {
                message = "Bu projeye ait fotograf bulunamadi."
            });
        }

        // Yalnizca secilen fotograf kapak olsun.
        foreach (var image in project.Images)
        {
            image.IsCover = image.Id == imageId;
        }

        await _projectRepository.UpdateAsync(
            cancellationToken);

        return Ok(new
        {
            projectId = id,
            coverImageId = imageId,
            message = "Kapak fotografi basariyla degistirildi."
        });
    }


}
