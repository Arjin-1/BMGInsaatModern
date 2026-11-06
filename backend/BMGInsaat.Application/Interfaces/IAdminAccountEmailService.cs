namespace BMGInsaat.Application.Interfaces;

public interface IAdminAccountEmailService
{
    Task SendAsync(
        string recipientEmail,
        string subject,
        string textBody,
        CancellationToken cancellationToken = default);
}
