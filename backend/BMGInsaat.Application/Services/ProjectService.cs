using BMGInsaat.Application.DTOs.Projects;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Services;

public class ProjectService : IProjectService
{

private readonly IProjectRepository _projectRepository;
private readonly ITranslationService _translationService;

public ProjectService(
    IProjectRepository projectRepository,
    ITranslationService translationService)
{
    _projectRepository = projectRepository;
    _translationService = translationService;
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

    var nameTr = request.NameTr.Trim();
    var descriptionTr = request.DescriptionTr?.Trim() ?? string.Empty;

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

    var project = await _projectRepository.GetByIdAsync(
        id,
        cancellationToken);

    if (project is null)
    {
        return false;
    }

    var nameTr = request.NameTr.Trim();
    var descriptionTr = request.DescriptionTr?.Trim() ?? string.Empty;

    // İngilizce ad boşsa Azure ile otomatik çevir.
    var nameEn = string.IsNullOrWhiteSpace(request.NameEn)
        ? await _translationService.TranslateToEnglishAsync(
            nameTr,
            cancellationToken)
        : request.NameEn.Trim();

    // İngilizce açıklama boşsa Azure ile otomatik çevir.
    var descriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn)
        ? string.IsNullOrWhiteSpace(descriptionTr)
            ? string.Empty
            : await _translationService.TranslateToEnglishAsync(
                descriptionTr,
                cancellationToken)
        : request.DescriptionEn.Trim();

    project.NameTr = nameTr;
    project.NameEn = nameEn;
    project.DescriptionTr = descriptionTr;
    project.DescriptionEn = descriptionEn;

    project.City = request.City.Trim();
    project.Status = request.Status;
    project.IsPublished = request.IsPublished;
    project.UpdatedAtUtc = DateTime.UtcNow;

    await _projectRepository.UpdateAsync(cancellationToken);

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

    await _projectRepository.DeleteAsync(
        project,
        cancellationToken);

    return true;
}


}
