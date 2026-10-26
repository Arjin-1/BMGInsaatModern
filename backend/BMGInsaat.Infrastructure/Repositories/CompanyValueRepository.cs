using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using BMGInsaat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMGInsaat.Infrastructure.Repositories;

public class CompanyValueRepository : ICompanyValueRepository
{
    private readonly AppDbContext _context;

    public CompanyValueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompanyValue>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CompanyValues
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    
public async Task<IReadOnlyList<CompanyValue>> GetPublishedAsync(
    CancellationToken cancellationToken = default)
{
    return await _context.CompanyValues
        .AsNoTracking()
        .Where(x => x.IsPublished)
        .OrderBy(x => x.DisplayOrder)
        .ThenBy(x => x.Id)
        .ToListAsync(cancellationToken);
}


    public async Task<CompanyValue?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.CompanyValues
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        CompanyValue companyValue,
        CancellationToken cancellationToken = default)
    {
        await _context.CompanyValues.AddAsync(
            companyValue,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        CompanyValue companyValue,
        CancellationToken cancellationToken = default)
    {
        _context.CompanyValues.Remove(companyValue);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
