using BMGInsaat.Application.DTOs.Projects;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Services;

public class ProjectService : IProjectService
{

    private readonly IProjectRepository _projectRepository;
    private readonly ITranslationService _translationService;
    private readonly IProjectImageStorage _projectImageStorage;

    public ProjectService(
        IProjectRepository projectRepository,
        ITranslationService translationService,
        IProjectImageStorage projectImageStorage)
    {
        _projectRepository = projectRepository;
        _translationService = translationService;
        _projectImageStorage = projectImageStorage;
    }



    public async Task<int> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NameTr))
        {
            throw new ArgumentException(
                "Proje adi bos olamaz.");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            throw new ArgumentException(
                "Sehir bilgisi bos olamaz.");
        }

        if (!Enum.IsDefined(request.Status))
        {
            throw new ArgumentException(
                "Gecersiz proje durumu.");
        }

        if (request.StartDate.HasValue &&
        request.EndDate.HasValue &&
        request.EndDate.Value < request.StartDate.Value)
        {
            throw new ArgumentException(
                "Bitis tarihi baslangic tarihinden once olamaz.");
        }

        var nameTr = request.NameTr.Trim();
        var descriptionTr = request.DescriptionTr?.Trim() ?? string.Empty;

        var employerTr = request.EmployerTr?.Trim() ?? string.Empty;

        var employerEn = string.IsNullOrWhiteSpace(request.EmployerEn)
        ? string.IsNullOrWhiteSpace(employerTr)
            ? string.Empty
            : await _translationService.TranslateToEnglishAsync(
                employerTr, cancellationToken)
        : request.EmployerEn.Trim();

        // İngilizce proje adı boşsa Azure ile çevir.
        var nameEn = string.IsNullOrWhiteSpace(request.NameEn)
            ? await _translationService.TranslateToEnglishAsync(
                nameTr,
                cancellationToken)
            : request.NameEn.Trim();

        // İngilizce açıklama boşsa Azure ile çevir.
        var descriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn)
            ? string.IsNullOrWhiteSpace(descriptionTr)
                ? string.Empty
                : await _translationService.TranslateToEnglishAsync(
                    descriptionTr,
                    cancellationToken)
            : request.DescriptionEn.Trim();

        var project = new Project
        {
            NameTr = nameTr,
            NameEn = nameEn,
            DescriptionTr = descriptionTr,
            DescriptionEn = descriptionEn,
            City = request.City.Trim(),
            EmployerTr = employerTr,
            EmployerEn = employerEn,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsFeatured = request.IsFeatured,
            Status = request.Status,
            IsPublished = request.IsPublished,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await _projectRepository.AddAsync(
            project,
            cancellationToken);
    }




    public async Task<bool> UpdateAsync(
        int id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        // Proje adı kontrolü
        if (string.IsNullOrWhiteSpace(request.NameTr))
        {
            throw new ArgumentException(
                "Proje adi bos olamaz.");
        }

        // Şehir kontrolü
        if (string.IsNullOrWhiteSpace(request.City))
        {
            throw new ArgumentException(
                "Sehir bilgisi bos olamaz.");
        }

        // Proje durumu kontrolü
        if (!Enum.IsDefined(request.Status))
        {
            throw new ArgumentException(
                "Gecersiz proje durumu.");
        }

        // Tarih kontrolü
        if (request.StartDate.HasValue &&
            request.EndDate.HasValue &&
            request.EndDate.Value < request.StartDate.Value)
        {
            throw new ArgumentException(
                "Bitis tarihi baslangic tarihinden once olamaz.");
        }

        // Mevcut projeyi veritabanından getir
        var project = await _projectRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (project is null)
        {
            return false;
        }

        // Türkçe içerikleri hazırla
        var nameTr = request.NameTr.Trim();

        var descriptionTr =
            request.DescriptionTr?.Trim() ?? string.Empty;

        var employerTr =
            request.EmployerTr?.Trim() ?? string.Empty;


        // -------------------------------------------------
        // İŞVEREN KURUMU - OTOMATİK İNGİLİZCE ÇEVİRİ
        // -------------------------------------------------

        var employerChanged = !string.Equals(
            employerTr,
            project.EmployerTr,
            StringComparison.Ordinal);

        var employerEnUnchanged = string.Equals(
            request.EmployerEn?.Trim() ?? string.Empty,
            project.EmployerEn,
            StringComparison.Ordinal);

        string employerEn;

        if (string.IsNullOrWhiteSpace(employerTr))
        {
            employerEn = string.Empty;
        }
        else if (
            string.IsNullOrWhiteSpace(request.EmployerEn) ||
            (employerChanged && employerEnUnchanged))
        {
            employerEn =
                await _translationService.TranslateToEnglishAsync(
                    employerTr,
                    cancellationToken);
        }
        else
        {
            employerEn = request.EmployerEn!.Trim();
        }


        // -------------------------------------------------
        // PROJE ADI - OTOMATİK İNGİLİZCE ÇEVİRİ
        // -------------------------------------------------

        var nameChanged = !string.Equals(
            nameTr,
            project.NameTr,
            StringComparison.Ordinal);

        var nameEnUnchanged = string.Equals(
            request.NameEn?.Trim() ?? string.Empty,
            project.NameEn,
            StringComparison.Ordinal);

        string nameEn;

        if (
            string.IsNullOrWhiteSpace(request.NameEn) ||
            (nameChanged && nameEnUnchanged))
        {
            nameEn =
                await _translationService.TranslateToEnglishAsync(
                    nameTr,
                    cancellationToken);
        }
        else
        {
            nameEn = request.NameEn!.Trim();
        }


        // -------------------------------------------------
        // PROJE AÇIKLAMASI - OTOMATİK İNGİLİZCE ÇEVİRİ
        // -------------------------------------------------

        var descriptionChanged = !string.Equals(
            descriptionTr,
            project.DescriptionTr,
            StringComparison.Ordinal);

        var descriptionEnUnchanged = string.Equals(
            request.DescriptionEn?.Trim() ?? string.Empty,
            project.DescriptionEn,
            StringComparison.Ordinal);

        string descriptionEn;

        if (string.IsNullOrWhiteSpace(descriptionTr))
        {
            descriptionEn = string.Empty;
        }
        else if (
            string.IsNullOrWhiteSpace(request.DescriptionEn) ||
            (descriptionChanged && descriptionEnUnchanged))
        {
            descriptionEn =
                await _translationService.TranslateToEnglishAsync(
                    descriptionTr,
                    cancellationToken);
        }
        else
        {
            descriptionEn = request.DescriptionEn!.Trim();
        }


        // -------------------------------------------------
        // PROJE BİLGİLERİNİ GÜNCELLE
        // -------------------------------------------------

        project.NameTr = nameTr;
        project.NameEn = nameEn;

        project.DescriptionTr = descriptionTr;
        project.DescriptionEn = descriptionEn;

        project.City = request.City.Trim();

        project.EmployerTr = employerTr;
        project.EmployerEn = employerEn;

        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;

        project.IsFeatured = request.IsFeatured;
        project.Status = request.Status;
        project.IsPublished = request.IsPublished;

        project.UpdatedAtUtc = DateTime.UtcNow;


        // -------------------------------------------------
        // DEĞİŞİKLİKLERİ POSTGRESQL'E KAYDET
        // -------------------------------------------------

        await _projectRepository.UpdateAsync(
            cancellationToken);

        return true;
    }





    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return false;
        }

        var project = await _projectRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (project is null)
        {
            return false;
        }

        // Silinecek projeye ait fotograf adreslerini sakla.
        var imageUrls = project.Images
            .Select(image => image.ImageUrl)
            .Where(url => url.StartsWith(
                "/uploads/projects/",
                StringComparison.Ordinal))
            .Distinct()
            .ToList();

        // Once veritabanindaki projeyi sil.
        await _projectRepository.DeleteAsync(
            project,
            cancellationToken);

        // Veritabani islemi basariliysa fiziksel dosyalari sil.
        foreach (var imageUrl in imageUrls)
        {
            await _projectImageStorage.DeleteAsync(
                imageUrl,
                cancellationToken);
        }

        return true;
    }



}
