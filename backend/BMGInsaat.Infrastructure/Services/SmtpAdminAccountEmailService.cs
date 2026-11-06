using BMGInsaat.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BMGInsaat.Infrastructure.Services;

public class SmtpAdminAccountEmailService : IAdminAccountEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpAdminAccountEmailService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string textBody,
        CancellationToken cancellationToken = default)
    {
        // SMTP ayarlarini guvenli yapilandirmadan oku
        var host = _configuration["AdminEmail:SmtpHost"]
            ?? throw new InvalidOperationException(
                "Admin SMTP sunucusu tanimlanmamis.");

        var port = _configuration.GetValue<int>(
            "AdminEmail:SmtpPort", 587);

        var username = _configuration["AdminEmail:Username"]
            ?? throw new InvalidOperationException(
                "Admin SMTP kullanici adi tanimlanmamis.");

        var password = _configuration["AdminEmail:Password"]
            ?? throw new InvalidOperationException(
                "Admin SMTP sifresi tanimlanmamis.");

        var fromEmail = _configuration["AdminEmail:FromEmail"]
            ?? throw new InvalidOperationException(
                "Admin gonderici e-posta adresi tanimlanmamis.");

        if (string.IsNullOrWhiteSpace(recipientEmail) ||
            string.IsNullOrWhiteSpace(subject) ||
            string.IsNullOrWhiteSpace(textBody))
        {
            throw new ArgumentException(
                "E-posta alicisi, konusu ve icerigi bos olamaz.");
        }

        // E-posta mesajini hazirla
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                "BMG Insaat Yonetim Sistemi",
                fromEmail));

        message.To.Add(
            new MailboxAddress(
                "BMG Insaat Yoneticisi",
                recipientEmail));

        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = textBody
        };

        // TLS korumali SMTP baglantisi
        using var client = new SmtpClient
        {
            Timeout = 15000
        };

        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            username,
            password,
            cancellationToken);

        await client.SendAsync(
            message,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);
    }
}
