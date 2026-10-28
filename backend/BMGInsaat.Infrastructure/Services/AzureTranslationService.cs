using System.Net.Http.Json;
using System.Text.Json;
using BMGInsaat.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BMGInsaat.Infrastructure.Services;

public class AzureTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AzureTranslationService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> TranslateToEnglishAsync(
        string turkishText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(turkishText))
        {
            return string.Empty;
        }

        var key = _configuration["Translator:Key"];
        var region = _configuration["Translator:Region"];
        var endpoint = _configuration["Translator:Endpoint"];

        if (string.IsNullOrWhiteSpace(key) ||
            string.IsNullOrWhiteSpace(region) ||
            string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "Azure Translator ayarlari eksik.");
        }

        var url = endpoint.TrimEnd('/') +
            "/translate?api-version=3.0&from=tr&to=en";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Add(
            "Ocp-Apim-Subscription-Key",
            key);

        request.Headers.Add(
            "Ocp-Apim-Subscription-Region",
            region);

        request.Content = JsonContent.Create(
            new[]
            {
                new { Text = turkishText }
            });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync(
                cancellationToken);

            var contentType = response.Content.Headers.ContentType?.ToString()
                ?? "Unknown";

            throw new HttpRequestException(
                $"TRANSLATOR_DIAGNOSTIC_V2 | " +
                $"HTTP {(int)response.StatusCode} | " +
                $"Content-Type: {contentType} | " +
                $"Response: {errorDetails}");
        }

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        using var json = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);

        var translatedText = json.RootElement[0]
            .GetProperty("translations")[0]
            .GetProperty("text")
            .GetString();

        return translatedText ?? string.Empty;
    }
}
