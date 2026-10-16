namespace BMGInsaat.Application.Interfaces;

public interface ITranslationService
{
    Task<string> TranslateToEnglishAsync(
        string turkishText,
        CancellationToken cancellationToken = default);
}
