using BMGInsaat.Application.DTOs.Contact;

namespace BMGInsaat.Application.Interfaces;

public interface IContactEmailService
{
    Task SendAsync(
        SendContactMessageRequest request,
        CancellationToken cancellationToken = default);
}
