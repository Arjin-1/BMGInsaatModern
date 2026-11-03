namespace BMGInsaat.Application.Interfaces;

public interface IProjectVideoStorage
{
    // Proje videosunu sunucuya kaydeder.
    Task<string> SaveAsync(
        Stream fileStream,
        string extension,
        CancellationToken cancellationToken = default);

    // Proje videosunu sunucudan siler.
    Task DeleteAsync(
        string videoUrl,
        CancellationToken cancellationToken = default);

    // Kaydedilen videoyu oynatma icin acar.
    // Dosya bulunamazsa null dondurur.
    Stream? OpenRead(string videoUrl);
}
