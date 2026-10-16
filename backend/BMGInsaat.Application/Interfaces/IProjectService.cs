using BMGInsaat.Application.DTOs.Projects;

namespace BMGInsaat.Application.Interfaces;

public interface IProjectService
{
    Task<int> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default);


    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

}
