namespace BMGInsaat.Application.Interfaces;

public interface ICertificateImageStorage
{
    // Sertifika gorselini sunucuya kaydeder.
    Task<string> SaveAsync(
        Stream fileStream,
        string extension,
        CancellationToken cancellationToken = default);

    // Sertifika gorselini sunucudan siler.
    Task DeleteAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);
}
