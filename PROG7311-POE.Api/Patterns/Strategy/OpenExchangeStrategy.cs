//Code attribution
//Refactoring.Guru. 2024. Strategy Design Pattern.
//Available at: https://refactoring.guru/design-patterns/strategy
//[Accessed: 22 April 2025]
//
//Code attribution
//OpenExchangeRates. 2024. API Documentation.
//Available at: https://docs.openexchangerates.org/reference/api-introduction
//[Accessed: 22 April 2025]

using System.Text.Json;

namespace PROG7311_POE.Patterns.Strategy;

/// <summary>
/// Concrete Strategy — OpenExchangeRates provider.
/// Uses the OpenExchangeRates REST API to perform live currency conversion.
/// Decimal arithmetic is used throughout to prevent floating-point errors.
/// </summary>
public class OpenExchangeStrategy : ICurrencyStrategy
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public OpenExchangeStrategy(HttpClient httpClient, string apiKey,
        string baseUrl = "https://openexchangerates.org/api/")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey     = apiKey     ?? throw new ArgumentNullException(nameof(apiKey));
        _baseUrl    = baseUrl;
    }

    public string GetProviderName() => "OpenExchangeRates";

    // Code attribution
    // Microsoft. 2023. IHttpClientFactory with .NET.
    // Available at: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory
    // [Accessed: 22 April 2025]
    // Stack Overflow. 2019. How to properly use HttpClient in .NET Core.
    // Available at: https://stackoverflow.com/questions/51478525/httpclient-this-instance-has-already-started
    // [Accessed: 22 April 2025]
    public async Task<decimal> Convert(decimal amount, string from, string to)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException("Source currency required.", nameof(from));
        if (string.IsNullOrWhiteSpace(to))   throw new ArgumentException("Target currency required.", nameof(to));

        try
        {
            var url = $"{_baseUrl}latest.json?app_id={_apiKey}&base={from}&symbols={to}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var rate = doc.RootElement
                .GetProperty("rates")
                .GetProperty(to.ToUpperInvariant())
                .GetDecimal();

            return Math.Round(amount * rate, 2);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"[{GetProviderName()}] Network error during currency conversion: {ex.Message}", ex);
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidOperationException(
                $"[{GetProviderName()}] Currency '{to}' not found in API response.");
        }
    }
}
