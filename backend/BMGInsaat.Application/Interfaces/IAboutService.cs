using BMGInsaat.Application.DTOs.About;
using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Interfaces;

public interface IAboutService
{
    Task<AboutContent?> GetAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        UpdateAboutContentRequest request,
        CancellationToken cancellationToken = default);
}
