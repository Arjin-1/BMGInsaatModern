namespace BMGInsaat.Application.Interfaces;

public interface IConstructionServiceImageStorage
{
    // Hizmet gorselini sunucuya kaydeder.
    Task<string> SaveAsync(
        Stream fileStream,
        string extension,
        CancellationToken cancellationToken = default);

    // Hizmet gorselini sunucudan siler.
    Task DeleteAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);
}
