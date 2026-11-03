using BMGInsaat.Application.DTOs.ProjectVideos;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/project-videos")]
[Authorize(Roles = "SuperAdmin")]
public class AdminProjectVideosController : ControllerBase
{
    private readonly IProjectVideoRepository _videoRepository;
    private readonly IProjectVideoStorage _videoStorage;
    private readonly ITranslationService _translationService;

    public AdminProjectVideosController(
        IProjectVideoRepository videoRepository,
        IProjectVideoStorage videoStorage,
        ITranslationService translationService)
    {
        _videoRepository = videoRepository;
        _videoStorage = videoStorage;
        _translationService = translationService;
    }

    // Yonetici panelinde tum proje videolarini listele
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var videos = await _videoRepository
            .GetAllAsync(cancellationToken);

        var result = videos.Select(video => new
        {
            video.Id,
            video.TitleTr,
            video.TitleEn,
            video.DescriptionTr,
            video.DescriptionEn,
            video.VideoUrl,
            video.IsPublished,
            video.DisplayOrder,
            video.CreatedAtUtc,
            video.UpdatedAtUtc
        });

        return Ok(result);
    }

    // Yeni proje videosu olustur - Otomatik Ingilizce ceviri
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectVideoRequest request,
        CancellationToken cancellationToken)
    {
        // Turkce bilgileri al
        var titleTr = request.TitleTr?.Trim();
        var descriptionTr = request.DescriptionTr?.Trim();

        // Zorunlu alan kontrolu
        if (string.IsNullOrWhiteSpace(titleTr) ||
            string.IsNullOrWhiteSpace(descriptionTr))
        {
            return BadRequest(new
            {
                message = "Turkce baslik ve aciklama zorunludur."
            });
        }

        // Baslik karakter siniri
        if (titleTr.Length > 200)
        {
            return BadRequest(new
            {
                message = "Video basligi 200 karakteri asamaz."
            });
        }

        // Goruntulenme sirasi kontrolu
        if (request.DisplayOrder < 0)
        {
            return BadRequest(new
            {
                message = "Goruntulenme sirasi negatif olamaz."
            });
        }

        // Azure Translator ile otomatik Ingilizce ceviri
        var titleEn = await _translationService
            .TranslateToEnglishAsync(
                titleTr,
                cancellationToken);

        var descriptionEn = await _translationService
            .TranslateToEnglishAsync(
                descriptionTr,
                cancellationToken);

        // Ceviri sonucunu kontrol et
        if (string.IsNullOrWhiteSpace(titleEn) ||
            string.IsNullOrWhiteSpace(descriptionEn))
        {
            return StatusCode(502, new
            {
                message = "Otomatik ceviri tamamlanamadi. Video kaydedilmedi."
            });
        }

        titleEn = titleEn.Trim();
        descriptionEn = descriptionEn.Trim();

        if (titleEn.Length > 200)
        {
            return BadRequest(new
            {
                message = "Cevrilen video basligi 200 karakteri asiyor."
            });
        }

        // Yeni video kaydini olustur
        var video = new ProjectVideo
        {
            TitleTr = titleTr,
            TitleEn = titleEn,

            DescriptionTr = descriptionTr,
            DescriptionEn = descriptionEn,

            VideoUrl = string.Empty,
            IsPublished = false,

            DisplayOrder = request.DisplayOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        // PostgreSQL'e kaydet
        var videoId = await _videoRepository
            .CreateAsync(
                video,
                cancellationToken);

        return Ok(new
        {
            id = videoId,
            message = "Proje videosu otomatik ceviriyle basariyla olusturuldu."
        });
    }

    
    // Proje videosu bilgilerini guncelle - Otomatik Ingilizce ceviri
    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProjectVideoRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz proje videosu ID."
            });
        }

        var video = await _videoRepository
            .GetByIdAsync(id, cancellationToken);

        if (video is null)
        {
            return NotFound(new
            {
                message = "Proje videosu bulunamadi."
            });
        }

        // Turkce bilgileri al
        var titleTr = request.TitleTr?.Trim();
        var descriptionTr = request.DescriptionTr?.Trim();

        // Zorunlu alan kontrolu
        if (string.IsNullOrWhiteSpace(titleTr) ||
            string.IsNullOrWhiteSpace(descriptionTr))
        {
            return BadRequest(new
            {
                message = "Turkce baslik ve aciklama zorunludur."
            });
        }

        // Baslik karakter siniri
        if (titleTr.Length > 200)
        {
            return BadRequest(new
            {
                message = "Video basligi 200 karakteri asamaz."
            });
        }

        // Goruntulenme sirasi kontrolu
        if (request.DisplayOrder < 0)
        {
            return BadRequest(new
            {
                message = "Goruntulenme sirasi negatif olamaz."
            });
        }

        // MP4 dosyasi olmayan video yayinlanamaz
        if (request.IsPublished &&
            string.IsNullOrWhiteSpace(video.VideoUrl))
        {
            return BadRequest(new
            {
                message = "Video yayinlanmadan once MP4 dosyasi yuklenmelidir."
            });
        }

        // Mevcut Ingilizce bilgileri koru
        var titleEn = video.TitleEn;
        var descriptionEn = video.DescriptionEn;

        // Turkce baslik degistiyse yeniden cevir
        if (video.TitleTr != titleTr ||
            string.IsNullOrWhiteSpace(titleEn))
        {
            titleEn = await _translationService
                .TranslateToEnglishAsync(
                    titleTr,
                    cancellationToken);
        }

        // Turkce aciklama degistiyse yeniden cevir
        if (video.DescriptionTr != descriptionTr ||
            string.IsNullOrWhiteSpace(descriptionEn))
        {
            descriptionEn = await _translationService
                .TranslateToEnglishAsync(
                    descriptionTr,
                    cancellationToken);
        }

        // Ceviri sonucunu kontrol et
        if (string.IsNullOrWhiteSpace(titleEn) ||
            string.IsNullOrWhiteSpace(descriptionEn))
        {
            return StatusCode(502, new
            {
                message = "Otomatik ceviri tamamlanamadi. Video guncellenmedi."
            });
        }

        titleEn = titleEn.Trim();
        descriptionEn = descriptionEn.Trim();

        if (titleEn.Length > 200)
        {
            return BadRequest(new
            {
                message = "Cevrilen video basligi 200 karakteri asiyor."
            });
        }

        // Turkce ve Ingilizce bilgileri guncelle
        video.TitleTr = titleTr;
        video.TitleEn = titleEn;

        video.DescriptionTr = descriptionTr;
        video.DescriptionEn = descriptionEn;

        // Diger bilgileri guncelle
        video.DisplayOrder = request.DisplayOrder;
        video.IsPublished = request.IsPublished;
        video.UpdatedAtUtc = DateTime.UtcNow;

        // PostgreSQL'e kaydet
        await _videoRepository.UpdateAsync(
            cancellationToken);

        return Ok(new
        {
            video.Id,
            video.IsPublished,
            message = "Proje videosu basariyla guncellendi."
        });
    }

    
    // Proje videosuna MP4 yukle veya mevcut videoyu degistir
    [HttpPost("{id:int}/video")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(210_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 210_000_000)]
    public async Task<IActionResult> UploadVideo(
        int id,
        [FromForm] IFormFile? file,
        [FromServices] ILogger<AdminProjectVideosController> logger,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz proje videosu ID."
            });
        }

        var video = await _videoRepository
            .GetByIdAsync(id, cancellationToken);

        if (video is null)
        {
            return NotFound(new
            {
                message = "Proje videosu bulunamadi."
            });
        }

        // Eski video dosyasinin referansini sakla
        var oldVideoUrl = video.VideoUrl;

        const long maxFileSize = 200_000_000;

        if (file is null ||
            file.Length < 12 ||
            file.Length > maxFileSize)
        {
            return BadRequest(new
            {
                message = "Video gecerli olmali ve 200 MB sinirini asmamalidir."
            });
        }

        // Dosya uzantisini kontrol et
        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (extension != ".mp4")
        {
            return BadRequest(new
            {
                message = "Yalnizca MP4 dosyalari yuklenebilir."
            });
        }

        // MP4 dosya basligini kontrol et
        var header = new byte[12];
        var bytesRead = 0;

        await using (var input = file.OpenReadStream())
        {
            while (bytesRead < header.Length)
            {
                var read = await input.ReadAsync(
                    header.AsMemory(bytesRead),
                    cancellationToken);

                if (read == 0)
                {
                    break;
                }

                bytesRead += read;
            }
        }

        if (bytesRead < header.Length)
        {
            return BadRequest(new
            {
                message = "Gecersiz veya eksik video dosyasi."
            });
        }

        // MP4 kapsayicisinin ftyp imzasini kontrol et
        var isMp4 =
            header[4] == (byte)'f' &&
            header[5] == (byte)'t' &&
            header[6] == (byte)'y' &&
            header[7] == (byte)'p';

        if (!isMp4)
        {
            return BadRequest(new
            {
                message = "Dosya MP4 formatiyla uyusmuyor."
            });
        }

        string newVideoUrl;

        // Yeni MP4 dosyasini ozel depolama alanina kaydet
        await using (var stream = file.OpenReadStream())
        {
            newVideoUrl = await _videoStorage.SaveAsync(
                stream,
                extension,
                cancellationToken);
        }

        try
        {
            // Yeni dosya referansini PostgreSQL'e kaydet
            video.VideoUrl = newVideoUrl;
            video.UpdatedAtUtc = DateTime.UtcNow;

            await _videoRepository.UpdateAsync(
                cancellationToken);
        }
        catch
        {
            // Veritabani kaydi basarisizsa yeni dosyayi temizle
            try
            {
                await _videoStorage.DeleteAsync(
                    newVideoUrl,
                    CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                logger.LogError(
                    cleanupException,
                    "Proje videosu {VideoId} icin yeni dosya temizlenemedi.",
                    id);
            }

            throw;
        }

        // Yeni video kaydedildikten sonra eski videoyu sil
        if (!string.IsNullOrWhiteSpace(oldVideoUrl))
        {
            try
            {
                await _videoStorage.DeleteAsync(
                    oldVideoUrl,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Proje videosu {VideoId} icin eski dosya temizlenemedi.",
                    id);
            }
        }

        return Ok(new
        {
            video.Id,
            video.VideoUrl,
            message = "Proje videosu basariyla yuklendi."
        });
    }

    
    // Proje videosunu ve ona ait MP4 dosyasini sil
    [HttpDelete("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] ILogger<AdminProjectVideosController> logger,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz proje videosu ID."
            });
        }

        // Silinecek video kaydini bul
        var video = await _videoRepository
            .GetByIdAsync(id, cancellationToken);

        if (video is null)
        {
            return NotFound(new
            {
                message = "Proje videosu bulunamadi."
            });
        }

        // Silmeden once MP4 dosya referansini sakla
        var videoUrl = video.VideoUrl;

        // Once video kaydini PostgreSQL'den sil
        await _videoRepository.DeleteAsync(
            video,
            cancellationToken);

        // Ardindan fiziksel MP4 dosyasini temizle
        if (!string.IsNullOrWhiteSpace(videoUrl))
        {
            try
            {
                await _videoStorage.DeleteAsync(
                    videoUrl,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Proje videosu {VideoId} silindi ancak MP4 dosyasi temizlenemedi.",
                    id);

                return Ok(new
                {
                    id,
                    message = "Proje videosu silindi ancak MP4 dosyasi temizlenemedi.",
                    warning = true
                });
            }
        }

        return Ok(new
        {
            id,
            message = "Proje videosu basariyla silindi."
        });
    }



}
