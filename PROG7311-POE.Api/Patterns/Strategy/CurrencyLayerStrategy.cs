//Code attribution
//Refactoring.Guru. 2024. Strategy Design Pattern.
//Available at: https://refactoring.guru/design-patterns/strategy
//[Accessed: 22 April 2025]

using System.Text.Json;

namespace PROG7311_POE.Patterns.Strategy;

/// <summary>
/// Concrete Strategy — CurrencyLayer provider.
/// Alternative implementation that uses the CurrencyLayer API for currency conversion.
/// Decimal arithmetic is used throughout to prevent floating-point errors.
/// </summary>
public class CurrencyLayerStrategy : ICurrencyStrategy
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public CurrencyLayerStrategy(HttpClient httpClient, string apiKey,
        string baseUrl = "https://api.currencylayer.com/")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey     = apiKey     ?? throw new ArgumentNullException(nameof(apiKey));
        _baseUrl    = baseUrl;
    }

    public string GetProviderName() => "CurrencyLayer";

    public async Task<decimal> Convert(decimal amount, string from, string to)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException("Source currency required.", nameof(from));
        if (string.IsNullOrWhiteSpace(to))   throw new ArgumentException("Target currency required.", nameof(to));

        try
        {
            var url = $"{_baseUrl}live?access_key={_apiKey}&source={from}&currencies={to}&format=1";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var key  = $"{from.ToUpperInvariant()}{to.ToUpperInvariant()}";
            var rate = doc.RootElement
                .GetProperty("quotes")
                .GetProperty(key)
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
                $"[{GetProviderName()}] Currency pair '{from}{to}' not found in API response.");
        }
    }
}
