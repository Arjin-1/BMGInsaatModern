using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BMGInsaat.Application.Interfaces;

namespace BMGInsaat.Infrastructure.Services;

public sealed class GraphAdminAccountEmailService
    : IAdminAccountEmailService
{
    private readonly OutlookTokenService _outlookTokenService;
    private readonly IHttpClientFactory _httpClientFactory;

    public GraphAdminAccountEmailService(
        OutlookTokenService outlookTokenService,
        IHttpClientFactory httpClientFactory)
    {
        _outlookTokenService = outlookTokenService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string textBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail) ||
            string.IsNullOrWhiteSpace(subject) ||
            string.IsNullOrWhiteSpace(textBody))
        {
            throw new ArgumentException(
                "E-posta alicisi, konusu ve icerigi zorunludur.");
        }

        // Microsoft OAuth2 erisim belirtecini al
        var accessToken = await _outlookTokenService
            .GetAccessTokenAsync(cancellationToken);

        // Microsoft Graph e-posta icerigi
        var payload = new
        {
            message = new
            {
                subject,
                body = new
                {
                    contentType = "Text",
                    content = textBody
                },
                toRecipients = new[]
                {
                    new
                    {
                        emailAddress = new
                        {
                            address = recipientEmail.Trim()
                        }
                    }
                }
            },
            saveToSentItems = true
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://graph.microsoft.com/v1.0/me/sendMail");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Content = JsonContent.Create(payload);

        using var client = _httpClientFactory.CreateClient();

        using var response = await client.SendAsync(
            request,
            cancellationToken);

        // Graph basarili istege 202 Accepted dondurur
        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            throw new HttpRequestException(
                $"Microsoft Graph e-posta gonderimini kabul etmedi. HTTP {(int)response.StatusCode}.");
        }
    }
}
