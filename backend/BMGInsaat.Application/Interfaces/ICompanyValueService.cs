using BMGInsaat.Application.DTOs.About;

namespace BMGInsaat.Application.Interfaces;

public interface ICompanyValueService
{
    Task<IReadOnlyList<CompanyValueResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
    
    
    Task<IReadOnlyList<CompanyValueResponse>> GetPublishedAsync(
        CancellationToken cancellationToken = default);


    Task<CompanyValueResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CompanyValueResponse> CreateAsync(
        SaveCompanyValueRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        SaveCompanyValueRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
