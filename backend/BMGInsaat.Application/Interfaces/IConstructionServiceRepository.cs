using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Interfaces;

public interface IConstructionServiceRepository
{
    // Ziyaretciler: Yalnizca yayinlanmis hizmetler
    Task<List<ConstructionService>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    // Yonetici: Tum hizmetler
    Task<List<ConstructionService>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Yonetici: Yeni hizmet olustur
    Task<int> CreateAsync(
        ConstructionService service,
        CancellationToken cancellationToken = default);

    // Hizmeti ID ile bul
    Task<ConstructionService?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    // Hizmet degisikliklerini kaydet
    Task UpdateAsync(
        CancellationToken cancellationToken = default);

    // Hizmeti veritabanindan sil
    Task DeleteAsync(
        ConstructionService service,
        CancellationToken cancellationToken = default);
}
