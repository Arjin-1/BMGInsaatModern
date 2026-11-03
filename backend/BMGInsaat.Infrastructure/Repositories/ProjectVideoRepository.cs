using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using BMGInsaat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMGInsaat.Infrastructure.Repositories;

public class ProjectVideoRepository : IProjectVideoRepository
{
    private readonly AppDbContext _context;

    public ProjectVideoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Ziyaretciler icin yalnizca yayinlanmis videolar
    public async Task<List<ProjectVideo>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ProjectVideos
            .AsNoTracking()
            .Where(video => video.IsPublished)
            .OrderBy(video => video.DisplayOrder)
            .ThenBy(video => video.Id)
            .ToListAsync(cancellationToken);
    }

    // Yonetici paneli icin tum videolar
    public async Task<List<ProjectVideo>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ProjectVideos
            .AsNoTracking()
            .OrderBy(video => video.DisplayOrder)
            .ThenBy(video => video.Id)
            .ToListAsync(cancellationToken);
    }

    // Yeni video kaydini veritabanina ekle
    public async Task<int> CreateAsync(
        ProjectVideo video,
        CancellationToken cancellationToken = default)
    {
        await _context.ProjectVideos.AddAsync(
            video,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return video.Id;
    }

    // Video kaydini ID ile bul
    public async Task<ProjectVideo?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProjectVideos
            .FirstOrDefaultAsync(
                video => video.Id == id,
                cancellationToken);
    }

    // Video bilgilerini veritabaninda guncelle
    public async Task UpdateAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Video kaydini veritabanindan sil
    public async Task DeleteAsync(
        ProjectVideo video,
        CancellationToken cancellationToken = default)
    {
        _context.ProjectVideos.Remove(video);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
