using BMGInsaat.Application.Interfaces;
using BMGInsaat.Application.DTOs.About;
using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Services;


public class CompanyValueService : ICompanyValueService

{
    private readonly ICompanyValueRepository _repository;
    private readonly ITranslationService _translationService;

    public CompanyValueService(
        ICompanyValueRepository repository,
        ITranslationService translationService)
    {
        _repository = repository;
        _translationService = translationService;
    }
    

public async Task<IReadOnlyList<CompanyValueResponse>> GetAllAsync(
    CancellationToken cancellationToken = default)
{
    var companyValues = await _repository.GetAllAsync(
        cancellationToken);

    return companyValues
        .Select(MapToResponse)
        .ToList();
}




public async Task<IReadOnlyList<CompanyValueResponse>> GetPublishedAsync(
    CancellationToken cancellationToken = default)
{
    var companyValues = await _repository.GetPublishedAsync(
        cancellationToken);

    return companyValues
        .Select(MapToResponse)
        .ToList();
}





public async Task<CompanyValueResponse?> GetByIdAsync(
    int id,
    CancellationToken cancellationToken = default)
{
    var companyValue = await _repository.GetByIdAsync(
        id,
        cancellationToken);

    if (companyValue is null)
    {
        return null;
    }

    return MapToResponse(companyValue);
}




public async Task<CompanyValueResponse> CreateAsync(
    SaveCompanyValueRequest request,
    CancellationToken cancellationToken = default)
{
    if (request is null)
        throw new ArgumentNullException(nameof(request));

    if (string.IsNullOrWhiteSpace(request.TitleTr))
        throw new ArgumentException("Deger basligi bos olamaz.");

    if (string.IsNullOrWhiteSpace(request.DescriptionTr))
        throw new ArgumentException("Deger aciklamasi bos olamaz.");

    var titleTr = request.TitleTr.Trim();
    var descriptionTr = request.DescriptionTr.Trim();

    var titleEn = string.IsNullOrWhiteSpace(request.TitleEn)
        ? await _translationService.TranslateToEnglishAsync(
            titleTr, cancellationToken)
        : request.TitleEn.Trim();

    var descriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn)
        ? await _translationService.TranslateToEnglishAsync(
            descriptionTr, cancellationToken)
        : request.DescriptionEn.Trim();

    var companyValue = new CompanyValue
    {
        TitleTr = titleTr,
        TitleEn = titleEn,
        DescriptionTr = descriptionTr,
        DescriptionEn = descriptionEn,
        DisplayOrder = request.DisplayOrder,
        IsPublished = request.IsPublished,
        CreatedAtUtc = DateTime.UtcNow
    };

    await _repository.AddAsync(companyValue, cancellationToken);

    return MapToResponse(companyValue);
}



public async Task<bool> UpdateAsync(
    int id,
    SaveCompanyValueRequest request,
    CancellationToken cancellationToken = default)
{
    if (request is null)
        throw new ArgumentNullException(nameof(request));

    if (string.IsNullOrWhiteSpace(request.TitleTr))
        throw new ArgumentException("Deger basligi bos olamaz.");

    if (string.IsNullOrWhiteSpace(request.DescriptionTr))
        throw new ArgumentException("Deger aciklamasi bos olamaz.");

    var companyValue = await _repository.GetByIdAsync(
        id,
        cancellationToken);

    if (companyValue is null)
        return false;

    var titleTr = request.TitleTr.Trim();
    var descriptionTr = request.DescriptionTr.Trim();

    var titleEn = string.IsNullOrWhiteSpace(request.TitleEn)
        ? await _translationService.TranslateToEnglishAsync(
            titleTr, cancellationToken)
        : request.TitleEn.Trim();

    var descriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn)
        ? await _translationService.TranslateToEnglishAsync(
            descriptionTr, cancellationToken)
        : request.DescriptionEn.Trim();

    companyValue.TitleTr = titleTr;
    companyValue.TitleEn = titleEn;
    companyValue.DescriptionTr = descriptionTr;
    companyValue.DescriptionEn = descriptionEn;
    companyValue.DisplayOrder = request.DisplayOrder;
    companyValue.IsPublished = request.IsPublished;
    companyValue.UpdatedAtUtc = DateTime.UtcNow;

    await _repository.UpdateAsync(cancellationToken);

    return true;
}

public async Task<bool> DeleteAsync(
    int id,
    CancellationToken cancellationToken = default)
{
    var companyValue = await _repository.GetByIdAsync(
        id,
        cancellationToken);

    if (companyValue is null)
    {
        return false;
    }

    await _repository.DeleteAsync(
        companyValue,
        cancellationToken);

    return true;
}


private static CompanyValueResponse MapToResponse(
    CompanyValue value)
{
    return new CompanyValueResponse
    {
        Id = value.Id,
        TitleTr = value.TitleTr,
        TitleEn = value.TitleEn,
        DescriptionTr = value.DescriptionTr,
        DescriptionEn = value.DescriptionEn,
        DisplayOrder = value.DisplayOrder,
        IsPublished = value.IsPublished,
        CreatedAtUtc = value.CreatedAtUtc,
        UpdatedAtUtc = value.UpdatedAtUtc
    };
}



}
