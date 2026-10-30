using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Interfaces;

public interface ICertificateRepository
{
    // Ziyaretciler: Yalnizca yayinlanmis sertifikalar
    Task<List<Certificate>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    // Yonetici: Tum sertifikalar
    Task<List<Certificate>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Yonetici: Yeni sertifika olustur
    Task<int> CreateAsync(
        Certificate certificate,
        CancellationToken cancellationToken = default);

    // Sertifikayi ID ile bul
    Task<Certificate?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    // Sertifika degisikliklerini kaydet
    Task UpdateAsync(
        CancellationToken cancellationToken = default);

    // Sertifikayi veritabanindan sil
    Task DeleteAsync(
        Certificate certificate,
        CancellationToken cancellationToken = default);
}
