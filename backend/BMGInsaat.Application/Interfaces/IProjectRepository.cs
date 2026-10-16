using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Interfaces;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    Task<Project?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<int> AddAsync(
        Project project,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        CancellationToken cancellationToken = default);


Task DeleteAsync(
    Project project,
    CancellationToken cancellationToken = default);

}
