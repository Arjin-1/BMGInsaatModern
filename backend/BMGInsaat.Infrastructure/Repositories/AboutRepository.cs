using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using BMGInsaat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMGInsaat.Infrastructure.Repositories;

public class AboutRepository : IAboutRepository
{
    private readonly AppDbContext _context;

    public AboutRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AboutContent?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AboutContents
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        AboutContent aboutContent,
        CancellationToken cancellationToken = default)
    {
        await _context.AboutContents.AddAsync(
            aboutContent,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
