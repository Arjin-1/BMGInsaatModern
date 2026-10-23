using BMGInsaat.Application.DTOs.About;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Services;

public class AboutService : IAboutService
{
    private readonly IAboutRepository _aboutRepository;
    private readonly ITranslationService _translationService;

    public AboutService(
        IAboutRepository aboutRepository,
        ITranslationService translationService)
    {
        _aboutRepository = aboutRepository;
        _translationService = translationService;
    }

    public async Task<AboutContent?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await _aboutRepository.GetAsync(cancellationToken);
    }

    public async Task SaveAsync(
        UpdateAboutContentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.AboutTr))
        {
            throw new ArgumentException(
                "Sirket tanitimi bos olamaz.");
        }

        if (string.IsNullOrWhiteSpace(request.MissionTr))
        {
            throw new ArgumentException(
                "Misyon bos olamaz.");
        }

        if (string.IsNullOrWhiteSpace(request.VisionTr))
        {
            throw new ArgumentException(
                "Vizyon bos olamaz.");
        }

        var aboutTr = request.AboutTr.Trim();
        var missionTr = request.MissionTr.Trim();
        var visionTr = request.VisionTr.Trim();

        // İngilizce içerikler boşsa otomatik çevir.
        var aboutEn = string.IsNullOrWhiteSpace(request.AboutEn)
            ? await _translationService.TranslateToEnglishAsync(
                aboutTr, cancellationToken)
            : request.AboutEn.Trim();

        var missionEn = string.IsNullOrWhiteSpace(request.MissionEn)
            ? await _translationService.TranslateToEnglishAsync(
                missionTr, cancellationToken)
            : request.MissionEn.Trim();

        var visionEn = string.IsNullOrWhiteSpace(request.VisionEn)
            ? await _translationService.TranslateToEnglishAsync(
                visionTr, cancellationToken)
            : request.VisionEn.Trim();

        var existingContent = await _aboutRepository.GetAsync(
            cancellationToken);

        if (existingContent is null)
        {
            var newContent = new AboutContent
            {
                AboutTr = aboutTr,
                AboutEn = aboutEn,
                MissionTr = missionTr,
                MissionEn = missionEn,
                VisionTr = visionTr,
                VisionEn = visionEn,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _aboutRepository.AddAsync(
                newContent, cancellationToken);

            return;
        }

        existingContent.AboutTr = aboutTr;
        existingContent.AboutEn = aboutEn;
        existingContent.MissionTr = missionTr;
        existingContent.MissionEn = missionEn;
        existingContent.VisionTr = visionTr;
        existingContent.VisionEn = visionEn;
        existingContent.UpdatedAtUtc = DateTime.UtcNow;

        await _aboutRepository.UpdateAsync(cancellationToken);
    }
}
