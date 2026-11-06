using BMGInsaat.Application.DTOs.ConstructionServices;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/services")]
[Authorize(Roles = "SuperAdmin")]
public class AdminConstructionServicesController : ControllerBase
{

    private readonly IConstructionServiceRepository _serviceRepository;
    private readonly IConstructionServiceImageStorage _serviceImageStorage;
    private readonly ITranslationService _translationService;

    public AdminConstructionServicesController(
        IConstructionServiceRepository serviceRepository,
        IConstructionServiceImageStorage serviceImageStorage,
        ITranslationService translationService)
    {
        _serviceRepository = serviceRepository;
        _serviceImageStorage = serviceImageStorage;
        _translationService = translationService;
    }


    // Yonetici panelinde tum hizmetleri listele
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var services = await _serviceRepository
            .GetAllAsync(cancellationToken);

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
            service.IsPublished,
            service.DisplayOrder,
            service.CreatedAtUtc,
            service.UpdatedAtUtc
        });

        return Ok(result);
    }


    // Yeni hizmet olustur - Otomatik Ingilizce ceviri
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromBody] CreateConstructionServiceRequest request,
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
                message = "Hizmet basligi 200 karakteri asamaz."
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
                message = "Otomatik ceviri tamamlanamadi. Hizmet kaydedilmedi."
            });
        }

        titleEn = titleEn.Trim();
        descriptionEn = descriptionEn.Trim();

        if (titleEn.Length > 200)
        {
            return BadRequest(new
            {
                message = "Cevrilen hizmet basligi 200 karakteri asiyor."
            });
        }

        // Hizmet kaydini olustur
        var service = new ConstructionService
        {
            TitleTr = titleTr,
            TitleEn = titleEn,

            DescriptionTr = descriptionTr,
            DescriptionEn = descriptionEn,

            ImageUrl = string.Empty,
            IsPublished = false,

            DisplayOrder = request.DisplayOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        // PostgreSQL'e kaydet
        var serviceId = await _serviceRepository
            .CreateAsync(
                service,
                cancellationToken);

        return Ok(new
        {
            id = serviceId,
            message = "Hizmet otomatik ceviriyle basariyla olusturuldu."
        });
    }




    // Hizmet bilgilerini guncelle - Otomatik Ingilizce ceviri
    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateConstructionServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz hizmet ID."
            });
        }

        var service = await _serviceRepository
            .GetByIdAsync(id, cancellationToken);

        if (service is null)
        {
            return NotFound(new
            {
                message = "Hizmet bulunamadi."
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
                message = "Hizmet basligi 200 karakteri asamaz."
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

        // Gorseli olmayan hizmet yayinlanamaz
        if (request.IsPublished &&
            string.IsNullOrWhiteSpace(service.ImageUrl))
        {
            return BadRequest(new
            {
                message = "Hizmet yayinlanmadan once gorsel yuklenmelidir."
            });
        }

        // Mevcut Ingilizce bilgileri koru
        var titleEn = service.TitleEn;
        var descriptionEn = service.DescriptionEn;

        // Turkce baslik degistiyse yeniden cevir
        if (service.TitleTr != titleTr ||
            string.IsNullOrWhiteSpace(titleEn))
        {
            titleEn = await _translationService
                .TranslateToEnglishAsync(
                    titleTr,
                    cancellationToken);
        }

        // Turkce aciklama degistiyse yeniden cevir
        if (service.DescriptionTr != descriptionTr ||
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
                message = "Otomatik ceviri tamamlanamadi. Hizmet guncellenmedi."
            });
        }

        titleEn = titleEn.Trim();
        descriptionEn = descriptionEn.Trim();

        if (titleEn.Length > 200)
        {
            return BadRequest(new
            {
                message = "Cevrilen hizmet basligi 200 karakteri asiyor."
            });
        }

        // Turkce ve Ingilizce bilgileri guncelle
        service.TitleTr = titleTr;
        service.TitleEn = titleEn;

        service.DescriptionTr = descriptionTr;
        service.DescriptionEn = descriptionEn;

        // Diger bilgileri guncelle
        service.DisplayOrder = request.DisplayOrder;
        service.IsPublished = request.IsPublished;
        service.UpdatedAtUtc = DateTime.UtcNow;

        // PostgreSQL'e kaydet
        await _serviceRepository.UpdateAsync(
            cancellationToken);

        return Ok(new
        {
            service.Id,
            service.IsPublished,
            message = "Hizmet basariyla guncellendi."
        });
    }



    // Hizmete gorsel yukle veya mevcut gorseli degistir
    [HttpPost("{id:int}/image")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadImage(
        int id,
        [FromForm] IFormFile? file,
        [FromServices] ILogger<AdminConstructionServicesController> logger,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz hizmet ID."
            });
        }

        var service = await _serviceRepository
            .GetByIdAsync(id, cancellationToken);

        if (service is null)
        {
            return NotFound(new
            {
                message = "Hizmet bulunamadi."
            });
        }

        // Eski gorsel adresini sakla
        var oldImageUrl = service.ImageUrl;

        const long maxFileSize = 15_000_000;

        if (file is null ||
            file.Length == 0 ||
            file.Length > maxFileSize)
        {
            return BadRequest(new
            {
                message = "Gorsel bos olamaz ve 15 MB sinirini asamaz."
            });
        }

        // Dosya uzantisini kontrol et
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

        // Dosyanin ilk 12 byte'ini oku
        var header = new byte[12];

        await using (var input = file.OpenReadStream())
        {
            var bytesRead = 0;

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

            if (bytesRead < header.Length)
            {
                return BadRequest(new
                {
                    message = "Gecersiz gorsel dosyasi."
                });
            }
        }

        // JPEG dosya imzasi
        var isJpeg =
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF;

        // PNG dosya imzasi
        var isPng =
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A;

        // WebP dosya imzasi
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
                message = "Dosya icerigi gorsel formatiyla uyusmuyor."
            });
        }

        string imageUrl;

        // Yeni gorseli sunucuya kaydet
        await using (var stream = file.OpenReadStream())
        {
            imageUrl = await _serviceImageStorage.SaveAsync(
                stream,
                extension,
                cancellationToken);
        }

        try
        {
            // Yeni gorsel adresini veritabanina kaydet
            service.ImageUrl = imageUrl;
            service.UpdatedAtUtc = DateTime.UtcNow;

            await _serviceRepository.UpdateAsync(
                cancellationToken);
        }
        catch
        {
            // Veritabani kaydi basarisizsa yeni dosyayi temizle
            await _serviceImageStorage.DeleteAsync(
                imageUrl,
                CancellationToken.None);

            throw;
        }

        // Yeni gorsel kaydedildikten sonra eskisini sil
        if (!string.IsNullOrWhiteSpace(oldImageUrl))
        {
            try
            {
                await _serviceImageStorage.DeleteAsync(
                    oldImageUrl,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Hizmet {ServiceId} icin eski gorsel temizlenemedi.",
                    service.Id);
            }
        }

        return Ok(new
        {
            service.Id,
            ImageUrl = $"/api/services/{service.Id}/image",
            message = "Hizmet gorseli basariyla kaydedildi."
        });
    }


    // Hizmeti ve ona ait gorseli sil
    [HttpDelete("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] ILogger<AdminConstructionServicesController> logger,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz hizmet ID."
            });
        }

        // Silinecek hizmeti bul
        var service = await _serviceRepository
            .GetByIdAsync(id, cancellationToken);

        if (service is null)
        {
            return NotFound(new
            {
                message = "Hizmet bulunamadi."
            });
        }

        // Silmeden once gorsel adresini sakla
        var imageUrl = service.ImageUrl;

        // Once hizmet kaydini veritabanindan sil
        await _serviceRepository.DeleteAsync(
            service,
            cancellationToken);

        // Ardindan fiziksel gorsel dosyasini temizle
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            try
            {
                await _serviceImageStorage.DeleteAsync(
                    imageUrl,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Hizmet {ServiceId} silindi ancak gorsel temizlenemedi.",
                    id);

                return Ok(new
                {
                    id,
                    message = "Hizmet silindi ancak gorsel dosyasi temizlenemedi.",
                    warning = true
                });
            }
        }

        return Ok(new
        {
            id,
            message = "Hizmet basariyla silindi."
        });
    }



}
