using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Interfaces;

public interface ICompanyValueRepository
{
    Task<IReadOnlyList<CompanyValue>> GetAllAsync(
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<CompanyValue>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    Task<CompanyValue?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CompanyValue companyValue,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        CompanyValue companyValue,
        CancellationToken cancellationToken = default);
}
