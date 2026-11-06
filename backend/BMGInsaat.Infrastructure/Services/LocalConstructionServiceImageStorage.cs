using BMGInsaat.Application.Interfaces;

namespace BMGInsaat.Infrastructure.Services;

public class LocalConstructionServiceImageStorage
    : IConstructionServiceImageStorage
{
    private const string UrlPrefix = "/uploads/services/";

    private readonly string _storagePath;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    public LocalConstructionServiceImageStorage(
        string contentRootPath)
    {
        _storagePath = Path.Combine(
            contentRootPath,
            "private-uploads",
            "services");
    }

    // Hizmet gorselini sunucuya kaydet
    public async Task<string> SaveAsync(
        Stream fileStream,
        string extension,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException(
                "Gorsel uzantisi bos olamaz.");
        }

        extension = extension.Trim().ToLowerInvariant();

        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Desteklenmeyen hizmet gorseli uzantisi.");
        }

        // Hizmet gorselleri icin klasor olustur
        Directory.CreateDirectory(_storagePath);

        // Benzersiz dosya adi olustur
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
                FileShare.None);

            await fileStream.CopyToAsync(
                output,
                cancellationToken);
        }
        catch
        {
            // Yukleme basarisizsa yarim kalan dosyayi sil
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }

        return UrlPrefix + fileName;
    }

    // Hizmet gorselini sunucudan sil
    public Task DeleteAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !imageUrl.StartsWith(
                UrlPrefix,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Gecersiz hizmet gorseli adresi.");
        }

        var fileName = imageUrl[UrlPrefix.Length..];

 
        // Klasor disina erisimi engelle ve
        // yalnizca sistemin olusturdugu GUID dosya adlarini kabul et.
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName != Path.GetFileName(fileName) ||
            !Guid.TryParseExact(
                Path.GetFileNameWithoutExtension(fileName),
                "N",
                out _))
        {
            throw new ArgumentException(
                "Gecersiz dosya adi.");
        }


        var extension = Path.GetExtension(fileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Gecersiz hizmet gorseli uzantisi.");
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

    
public Stream? OpenRead(string imageUrl)
{
    if (string.IsNullOrWhiteSpace(imageUrl) ||
        !imageUrl.StartsWith(
            UrlPrefix,
            StringComparison.Ordinal))
    {
        throw new ArgumentException(
            "Gecersiz hizmet gorseli adresi.");
    }

    var fileName = imageUrl[UrlPrefix.Length..];

    if (fileName != Path.GetFileName(fileName) ||
        !Guid.TryParseExact(
            Path.GetFileNameWithoutExtension(fileName),
            "N",
            out _) ||
        !AllowedExtensions.Contains(
            Path.GetExtension(fileName)))
    {
        throw new ArgumentException(
            "Gecersiz hizmet gorseli dosya adi.");
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
