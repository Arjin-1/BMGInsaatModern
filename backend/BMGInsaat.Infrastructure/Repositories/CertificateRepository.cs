using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using BMGInsaat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMGInsaat.Infrastructure.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly AppDbContext _context;

    public CertificateRepository(AppDbContext context)
    {
        _context = context;
    }

    // Ziyaretciler icin yalnizca yayinlanmis sertifikalar
    public async Task<List<Certificate>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Certificates
            .AsNoTracking()
            .Where(certificate => certificate.IsPublished)
            .OrderBy(certificate => certificate.DisplayOrder)
            .ThenBy(certificate => certificate.Id)
            .ToListAsync(cancellationToken);
    }

    // Yonetici paneli icin tum sertifikalar
    public async Task<List<Certificate>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Certificates
            .AsNoTracking()
            .OrderBy(certificate => certificate.DisplayOrder)
            .ThenBy(certificate => certificate.Id)
            .ToListAsync(cancellationToken);
    }

    // Yeni sertifikayi veritabanina kaydet
    public async Task<int> CreateAsync(
        Certificate certificate,
        CancellationToken cancellationToken = default)
    {
        await _context.Certificates.AddAsync(
            certificate,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return certificate.Id;
    }

    // Sertifikayi ID ile bul
    public async Task<Certificate?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Certificates
            .FirstOrDefaultAsync(
                certificate => certificate.Id == id,
                cancellationToken);
    }

    // Sertifikadaki degisiklikleri veritabanina kaydet
    public async Task UpdateAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Sertifikayi veritabanindan sil
    public async Task DeleteAsync(
        Certificate certificate,
        CancellationToken cancellationToken = default)
    {
        _context.Certificates.Remove(certificate);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
