
using System.Text;
using BMGInsaat.Application.DTOs.Contact;
using BMGInsaat.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BMGInsaat.Infrastructure.Services;

public class SmtpContactEmailService : IContactEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpContactEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(
        SendContactMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        // SMTP ayarlari uygulama yapilandirmasindan okunur.
        // Sifreler kaynak kodunda tutulmaz.

        var host = _configuration["ContactEmail:SmtpHost"]
            ?? throw new InvalidOperationException(
                "SMTP sunucu adresi tanimlanmamis.");

        var port = _configuration.GetValue<int>(
            "ContactEmail:SmtpPort", 587);

        var username = _configuration["ContactEmail:Username"]
            ?? throw new InvalidOperationException(
                "SMTP kullanici adi tanimlanmamis.");

        var password = _configuration["ContactEmail:Password"]
            ?? throw new InvalidOperationException(
                "SMTP sifresi tanimlanmamis.");

        var fromEmail = _configuration["ContactEmail:FromEmail"]
            ?? throw new InvalidOperationException(
                "Gonderici e-posta adresi tanimlanmamis.");

        // BMG Insaat'in mesajlari alacagi adres.
        const string recipientEmail = "bmginsaat@hotmail.com";

        // Ziyaretcinin gonderdigi bilgileri duz metin olarak hazirla.
        var body = new StringBuilder();

        body.AppendLine("BMG INSAAT - YENI ILETISIM MESAJI");
        body.AppendLine("--------------------------------");
        body.AppendLine();

        body.AppendLine($"Ad Soyad: {request.FullName}");
        body.AppendLine($"E-posta: {request.Email}");
        body.AppendLine($"Telefon: {request.Phone ?? "Belirtilmedi"}");
        body.AppendLine($"Konu: {request.Subject}");
        body.AppendLine();

        body.AppendLine("MESAJ:");
        body.AppendLine(request.Message);

        // E-posta mesaji
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress("BMG Insaat Web Sitesi", fromEmail));

        message.To.Add(
            new MailboxAddress("BMG Insaat", recipientEmail));

        message.Subject = "BMG Insaat - Yeni Iletisim Formu Mesaji";

        // Yanitla butonuna basildiginda ziyaretciye yanit verilebilir.
        message.ReplyTo.Add(
            new MailboxAddress(request.FullName, request.Email));

        message.Body = new TextPart("plain")
        {
            Text = body.ToString()
        };

        // Guvenli SMTP baglantisi
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
