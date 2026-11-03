using BMGInsaat.Application.Interfaces;

namespace BMGInsaat.Infrastructure.Services;

public class LocalProjectVideoStorage : IProjectVideoStorage
{
    private const string VideoPrefix = "/project-videos/";

    private readonly string _storagePath;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4"
        };

    public LocalProjectVideoStorage(string contentRootPath)
    {
        // Videolar wwwroot disinda saklanir.
        // Yayin kontrolu API tarafindan yapilacak.
        _storagePath = Path.Combine(
            contentRootPath,
            "private-uploads",
            "project-videos");
    }

    // Video dosyasini sunucuya kaydet
    public async Task<string> SaveAsync(
        Stream fileStream,
        string extension,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException(
                "Video uzantisi bos olamaz.");
        }

        extension = extension.Trim().ToLowerInvariant();

        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Yalnizca MP4 videolari desteklenmektedir.");
        }

        Directory.CreateDirectory(_storagePath);

        // Orijinal dosya adini kullanma.
        // Rastgele, benzersiz dosya adi olustur.
        var fileName = $"{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(
            _storagePath,
            fileName);

        try
        {
            await using var output = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await fileStream.CopyToAsync(
                output,
                cancellationToken);
        }
        catch
        {
            // Yukleme yarida kalirsa dosyayi temizle.
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }

        // Bu adres public URL degil, dahili dosya referansidir.
        return VideoPrefix + fileName;
    }

    // Video dosyasini sunucudan sil
    public Task DeleteAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(videoUrl) ||
            !videoUrl.StartsWith(
                VideoPrefix,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Gecersiz proje videosu adresi.");
        }

        var fileName = videoUrl[VideoPrefix.Length..];

        // Yalnizca sistemin urettigi GUID.mp4 adlarini kabul et.
        if (fileName.Length != 36 ||
            !fileName.EndsWith(
                ".mp4",
                StringComparison.Ordinal) ||
            !Guid.TryParseExact(
                fileName[..32],
                "N",
                out _))
        {
            throw new ArgumentException(
                "Gecersiz proje videosu dosya adi.");
        }

        var filePath = Path.Combine(
            _storagePath,
            fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    
    // Kaydedilen MP4 videosunu oynatma icin acar.
    public Stream? OpenRead(string videoUrl)
    {
        if (string.IsNullOrWhiteSpace(videoUrl) ||
            !videoUrl.StartsWith(
                VideoPrefix,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Gecersiz proje videosu adresi.");
        }

        var fileName = videoUrl[VideoPrefix.Length..];

        // Yalnizca sistemin olusturdugu GUID.mp4
        // dosyalarina erisim izni ver.
        if (fileName.Length != 36 ||
            !fileName.EndsWith(
                ".mp4",
                StringComparison.Ordinal) ||
            !Guid.TryParseExact(
                fileName[..32],
                "N",
                out _))
        {
            throw new ArgumentException(
                "Gecersiz proje videosu dosya adi.");
        }

        var filePath = Path.Combine(
            _storagePath,
            fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        return new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);
    }

}
