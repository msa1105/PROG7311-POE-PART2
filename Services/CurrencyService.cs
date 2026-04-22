using System.Text.Json;

namespace PROG7311_POE.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _appId;

    public CurrencyService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ExternalServices:OpenExchangeRates:BaseUrl"]
                   ?? "https://openexchangerates.org/api/";
        _appId = configuration["ExternalServices:OpenExchangeRates:AppId"]
                 ?? throw new InvalidOperationException("OpenExchangeRates AppId is not configured.");
    }

    public async Task<decimal> GetUsdToZarRateAsync()
    {
        var endpoint = $"{_baseUrl}latest.json?app_id={_appId}&symbols=ZAR";
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var rate = doc.RootElement
            .GetProperty("rates")
            .GetProperty("ZAR")
            .GetDecimal();

        return rate;
    }

    public async Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
    {
        var rate = await GetUsdToZarRateAsync();
        return Math.Round(usdAmount * rate, 2);
    }
}
