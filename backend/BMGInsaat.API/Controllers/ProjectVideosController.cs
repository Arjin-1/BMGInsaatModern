using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/project-videos")]
public class ProjectVideosController : ControllerBase
{
    private readonly IProjectVideoRepository _videoRepository;
    private readonly IProjectVideoStorage _videoStorage;

    public ProjectVideosController(
        IProjectVideoRepository videoRepository,
        IProjectVideoStorage videoStorage)
    {
        _videoRepository = videoRepository;
        _videoStorage = videoStorage;
    }

    // Ziyaretciler: Yalnizca yayinlanmis proje videolarini listele
    [HttpGet]
    public async Task<IActionResult> GetPublishedVideos(
        CancellationToken cancellationToken)
    {
        var videos = await _videoRepository
            .GetPublishedAsync(cancellationToken);

        var result = videos.Select(video => new
        {
            video.Id,
            video.TitleTr,
            video.TitleEn,
            video.DescriptionTr,
            video.DescriptionEn,
            video.VideoUrl,
            video.DisplayOrder
        });

        return Ok(result);
    }

    // Ziyaretciler: Yalnizca yayinlanmis videolari oynat
    [HttpGet("{id:int}/stream")]
    public async Task<IActionResult> StreamVideo(
        int id,
        CancellationToken cancellationToken)
    {
        var video = await _videoRepository
            .GetByIdAsync(id, cancellationToken);

        // Yayinlanmamis videolar herkese kapali
        if (video is null ||
            !video.IsPublished ||
            string.IsNullOrWhiteSpace(video.VideoUrl))
        {
            return NotFound();
        }

        try
        {
            var stream = _videoStorage.OpenRead(video.VideoUrl);

            if (stream is null)
            {
                return NotFound();
            }

            // HTTP Range destegi: ileri-geri sarma
            return File(
                stream,
                "video/mp4",
                enableRangeProcessing: true);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}
