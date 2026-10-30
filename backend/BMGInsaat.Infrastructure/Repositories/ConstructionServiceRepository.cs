using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using BMGInsaat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMGInsaat.Infrastructure.Repositories;

public class ConstructionServiceRepository : IConstructionServiceRepository
{
    private readonly AppDbContext _context;

    public ConstructionServiceRepository(AppDbContext context)
    {
        _context = context;
    }

    // Ziyaretciler icin yalnizca yayinlanmis hizmetler
    public async Task<List<ConstructionService>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConstructionServices
            .AsNoTracking()
            .Where(service => service.IsPublished)
            .OrderBy(service => service.DisplayOrder)
            .ThenBy(service => service.Id)
            .ToListAsync(cancellationToken);
    }

    // Yonetici paneli icin tum hizmetler
    public async Task<List<ConstructionService>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConstructionServices
            .AsNoTracking()
            .OrderBy(service => service.DisplayOrder)
            .ThenBy(service => service.Id)
            .ToListAsync(cancellationToken);
    }

    // Yeni hizmeti veritabanina kaydet
    public async Task<int> CreateAsync(
        ConstructionService service,
        CancellationToken cancellationToken = default)
    {
        await _context.ConstructionServices.AddAsync(
            service,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return service.Id;
    }

    // Hizmeti ID ile bul
    public async Task<ConstructionService?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConstructionServices
            .FirstOrDefaultAsync(
                service => service.Id == id,
                cancellationToken);
    }

    // Hizmet degisikliklerini veritabanina kaydet
    public async Task UpdateAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Hizmeti veritabanindan sil
    public async Task DeleteAsync(
        ConstructionService service,
        CancellationToken cancellationToken = default)
    {
        _context.ConstructionServices.Remove(service);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
