using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Interfaces;

public interface IProjectVideoRepository
{
    // Ziyaretciler: Yalnizca yayinlanmis videolar
    Task<List<ProjectVideo>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    // Yonetici: Tum proje videolari
    Task<List<ProjectVideo>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Yonetici: Yeni video kaydi olustur
    Task<int> CreateAsync(
        ProjectVideo video,
        CancellationToken cancellationToken = default);

    // Video kaydini ID ile bul
    Task<ProjectVideo?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    // Video bilgilerini guncelle
    Task UpdateAsync(
        CancellationToken cancellationToken = default);

    // Video kaydini veritabanindan sil
    Task DeleteAsync(
        ProjectVideo video,
        CancellationToken cancellationToken = default);
}
