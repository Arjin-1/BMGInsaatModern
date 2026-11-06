using BMGInsaat.Application.Interfaces;

namespace BMGInsaat.Infrastructure.Services;

public class LocalProjectImageStorage : IProjectImageStorage
{
    private const string UrlPrefix = "/uploads/projects/";

    private readonly string _storagePath;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    public LocalProjectImageStorage(string contentRootPath)
    {
        _storagePath = Path.Combine(
            contentRootPath,
            "private-uploads",
            "projects");
    }

    public async Task<string> SaveAsync(
        Stream fileStream,
        string extension,
        CancellationToken cancellationToken = default)
    {
        extension = extension.Trim().ToLowerInvariant();

        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Desteklenmeyen fotograf uzantisi.");
        }

        Directory.CreateDirectory(_storagePath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(_storagePath, fileName);

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
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }

        return UrlPrefix + fileName;
    }

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
                "Gecersiz fotograf adresi.");
        }

        var fileName = imageUrl[UrlPrefix.Length..];


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
                "Gecersiz fotograf uzantisi.");
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
            "Gecersiz fotograf adresi.");
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
            "Gecersiz fotograf dosya adi.");
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
