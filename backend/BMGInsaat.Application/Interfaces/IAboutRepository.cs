using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Interfaces;

public interface IAboutRepository
{
    Task<AboutContent?> GetAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AboutContent aboutContent,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        CancellationToken cancellationToken = default);
}
