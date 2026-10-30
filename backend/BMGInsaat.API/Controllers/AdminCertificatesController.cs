using BMGInsaat.Application.DTOs.Certificates;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/certificates")]
[Authorize(Roles = "SuperAdmin")]
public class AdminCertificatesController : ControllerBase
{
    private readonly ICertificateRepository _certificateRepository;
    private readonly ICertificateImageStorage _certificateImageStorage;

    public AdminCertificatesController(
        ICertificateRepository certificateRepository,
        ICertificateImageStorage certificateImageStorage)
    {
        _certificateRepository = certificateRepository;
        _certificateImageStorage = certificateImageStorage;
    }

    // Yonetici panelinde tum sertifikalari listele
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var certificates = await _certificateRepository
            .GetAllAsync(cancellationToken);

        var result = certificates.Select(certificate => new
        {
            certificate.Id,
            certificate.NameTr,
            certificate.NameEn,
            certificate.ImageUrl,
            certificate.IsPublished,
            certificate.DisplayOrder,
            certificate.CreatedAtUtc,
            certificate.UpdatedAtUtc
        });

        return Ok(result);
    }

    // Yeni sertifika olustur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromBody] CreateCertificateRequest request,
        CancellationToken cancellationToken)
    {
        var nameTr = request.NameTr?.Trim();
        var nameEn = request.NameEn?.Trim();

        if (string.IsNullOrWhiteSpace(nameTr) ||
            string.IsNullOrWhiteSpace(nameEn))
        {
            return BadRequest(new
            {
                message = "Turkce ve Ingilizce sertifika adlari zorunludur."
            });
        }

        var certificate = new Certificate
        {
            NameTr = nameTr,
            NameEn = nameEn,
            ImageUrl = string.Empty,
            IsPublished = false,
            DisplayOrder = request.DisplayOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        var certificateId = await _certificateRepository
            .CreateAsync(certificate, cancellationToken);

        return Ok(new
        {
            id = certificateId,
            message = "Sertifika basariyla olusturuldu."
        });
    }

    // Sertifikaya gorsel yukle
    [HttpPost("{id:int}/image")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadImage(
        int id,
        [FromForm] IFormFile? file,
        [FromServices] ILogger<AdminCertificatesController> logger,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz sertifika ID."
            });
        }

        var certificate = await _certificateRepository
            .GetByIdAsync(id, cancellationToken);

        if (certificate is null)
        {
            return NotFound(new
            {
                message = "Sertifika bulunamadi."
            });
        }


        // Degistirme isleminden once eski gorselin adresini sakla
        var oldImageUrl = certificate.ImageUrl;


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

        // Dosyanin gercek imzasini kontrol et
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
                message = "Dosya icerigi gorsel formatiyla uyusmuyor."
            });
        }

        string imageUrl;

        // Dosyayi sertifikalar klasorune kaydet
        await using (var stream = file.OpenReadStream())
        {
            imageUrl = await _certificateImageStorage.SaveAsync(
                stream,
                extension,
                cancellationToken);
        }

        try
        {
            // Gorsel adresini veritabanina kaydet
            certificate.ImageUrl = imageUrl;
            certificate.UpdatedAtUtc = DateTime.UtcNow;

            await _certificateRepository.UpdateAsync(
                cancellationToken);
        }
        catch
        {
            // Veritabani kaydi basarisizsa yuklenen dosyayi temizle
            await _certificateImageStorage.DeleteAsync(
                imageUrl,
                CancellationToken.None);

            throw;
        }


        // Yeni gorsel veritabanina kaydedildikten sonra
        // eski gorseli sunucudan sil
        if (!string.IsNullOrWhiteSpace(oldImageUrl))
        {
            try
            {
                await _certificateImageStorage.DeleteAsync(
                    oldImageUrl,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Yeni gorsel kaydedildi. Eski dosya silinemediyse
                // yeni gorseli geri alma; hatayi logla.
                logger.LogWarning(
                    ex,
                    "Sertifika {CertificateId} icin eski gorsel temizlenemedi.",
                    certificate.Id);
            }
        }

        return Ok(new
        {
            certificate.Id,
            certificate.ImageUrl,
            message = "Sertifika gorseli basariyla kaydedildi."
        });

    }


    // Sertifika bilgilerini guncelle ve yayin durumunu yonet
    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCertificateRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz sertifika ID."
            });
        }

        var certificate = await _certificateRepository
            .GetByIdAsync(id, cancellationToken);

        if (certificate is null)
        {
            return NotFound(new
            {
                message = "Sertifika bulunamadi."
            });
        }

        var nameTr = request.NameTr?.Trim();
        var nameEn = request.NameEn?.Trim();

        if (string.IsNullOrWhiteSpace(nameTr) ||
            string.IsNullOrWhiteSpace(nameEn))
        {
            return BadRequest(new
            {
                message = "Turkce ve Ingilizce sertifika adlari zorunludur."
            });
        }

        // Gorseli olmayan sertifika yayinlanamaz
        if (request.IsPublished &&
            string.IsNullOrWhiteSpace(certificate.ImageUrl))
        {
            return BadRequest(new
            {
                message = "Sertifika yayinlanmadan once gorsel yuklenmelidir."
            });
        }

        certificate.NameTr = nameTr;
        certificate.NameEn = nameEn;
        certificate.DisplayOrder = request.DisplayOrder;
        certificate.IsPublished = request.IsPublished;
        certificate.UpdatedAtUtc = DateTime.UtcNow;

        await _certificateRepository.UpdateAsync(
            cancellationToken);

        return Ok(new
        {
            certificate.Id,
            certificate.IsPublished,
            message = "Sertifika basariyla guncellendi."
        });
    }


    // Sertifikayi ve ona ait gorseli sil
    [HttpDelete("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] ILogger<AdminCertificatesController> logger,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Gecersiz sertifika ID."
            });
        }

        var certificate = await _certificateRepository
            .GetByIdAsync(id, cancellationToken);

        if (certificate is null)
        {
            return NotFound(new
            {
                message = "Sertifika bulunamadi."
            });
        }

        var imageUrl = certificate.ImageUrl;

        // Once veritabanindaki sertifika kaydini sil
        await _certificateRepository.DeleteAsync(
            certificate,
            cancellationToken);

        // Ardindan fiziksel gorsel dosyasini temizle
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            try
            {
                await _certificateImageStorage.DeleteAsync(
                    imageUrl,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Sertifika {CertificateId} silindi ancak gorsel temizlenemedi.",
                    id);

                return Ok(new
                {
                    id,
                    message = "Sertifika silindi ancak gorsel dosyasi temizlenemedi.",
                    warning = true
                });
            }
        }

        return Ok(new
        {
            id,
            message = "Sertifika basariyla silindi."
        });
    }


}
