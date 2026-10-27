namespace BMGInsaat.Application.Interfaces;

public interface IProjectImageStorage
{
    Task<string> SaveAsync(
        Stream fileStream,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);
}
