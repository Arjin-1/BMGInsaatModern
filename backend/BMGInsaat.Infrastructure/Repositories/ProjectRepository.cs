using BMGInsaat.Application.Interfaces;
using BMGInsaat.Domain.Entities;
using BMGInsaat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMGInsaat.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Include(p => p.Images)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task<int> AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(
            project,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }

    public async Task UpdateAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }


public async Task DeleteAsync(
    Project project,
    CancellationToken cancellationToken = default)
{
    _context.Projects.Remove(project);

    await _context.SaveChangesAsync(cancellationToken);
}

}
