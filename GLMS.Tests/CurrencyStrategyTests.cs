using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using PROG7311_POE.Patterns.Strategy;

namespace GLMS.Tests;

/// <summary>
/// Tests for the Strategy Pattern (Financial Integration / Currency Conversion).
/// HttpClient is mocked via HttpMessageHandler to avoid live API calls.
/// </summary>
public class CurrencyStrategyTests
{
    // Builds a mocked HttpClient that always returns the given JSON body.
    private static HttpClient MockHttpClient(string jsonBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content    = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            });
        return new HttpClient(handlerMock.Object);
    }

    // --- OpenExchangeStrategy ---

    [Fact]
    public async Task OpenExchangeStrategy_Convert_ReturnsCorrectDecimal()
    {
        var json = """{"rates":{"ZAR":18.50}}""";
        var strategy = new OpenExchangeStrategy(MockHttpClient(json), "test-key");

        var result = await strategy.Convert(100m, "USD", "ZAR");

        Assert.Equal(1850.00m, result);
    }

    [Fact]
    public async Task OpenExchangeStrategy_Convert_UsesDecimalRounding()
    {
        var json = """{"rates":{"ZAR":18.7654321}}""";
        var strategy = new OpenExchangeStrategy(MockHttpClient(json), "test-key");

        var result = await strategy.Convert(1m, "USD", "ZAR");

        // Math.Round(..., 2) should give 18.77
        Assert.Equal(18.77m, result);
    }

    [Fact]
    public async Task OpenExchangeStrategy_HttpError_ThrowsInvalidOperationException()
    {
        var strategy = new OpenExchangeStrategy(
            MockHttpClient("{}", HttpStatusCode.Unauthorized), "bad-key");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => strategy.Convert(50m, "USD", "ZAR"));
    }

    [Fact]
    public void OpenExchangeStrategy_GetProviderName_ReturnsOpenExchangeRates()
    {
        var strategy = new OpenExchangeStrategy(new HttpClient(), "test-key");
        Assert.Equal("OpenExchangeRates", strategy.GetProviderName());
    }

    [Fact]
    public async Task OpenExchangeStrategy_ZeroAmount_ThrowsArgumentOutOfRangeException()
    {
        var strategy = new OpenExchangeStrategy(new HttpClient(), "test-key");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => strategy.Convert(0m, "USD", "ZAR"));
    }

    // --- CurrencyLayerStrategy ---

    [Fact]
    public void CurrencyLayerStrategy_GetProviderName_ReturnsCurrencyLayer()
    {
        var strategy = new CurrencyLayerStrategy(new HttpClient(), "test-key");
        Assert.Equal("CurrencyLayer", strategy.GetProviderName());
    }

    // --- FinancialIntegrationModule ---

    [Fact]
    public async Task FinancialIntegrationModule_ProcessInvoice_ContainsFormattedSummary()
    {
        var json     = """{"rates":{"ZAR":18.50}}""";
        var strategy = new OpenExchangeStrategy(MockHttpClient(json), "test-key");
        var module   = new FinancialIntegrationModule(strategy);

        var invoice = await module.ProcessInvoice(100m, "ZAR");

        Assert.Contains("USD", invoice);
        Assert.Contains("ZAR", invoice);
        Assert.Contains("OpenExchangeRates", invoice);
    }

    [Fact]
    public async Task FinancialIntegrationModule_SetStrategy_SwitchesToNewStrategy()
    {
        // Start with OpenExchange
        var openJson     = """{"rates":{"ZAR":18.50}}""";
        var openStrategy = new OpenExchangeStrategy(MockHttpClient(openJson), "key1");
        var module       = new FinancialIntegrationModule(openStrategy);

        var resultBefore = await module.ProcessInvoice(10m, "ZAR");
        Assert.Contains("OpenExchangeRates", resultBefore);

        // Swap to a mock CurrencyLayer strategy
        var layerMock = new Mock<ICurrencyStrategy>();
        layerMock.Setup(s => s.Convert(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(200m);
        layerMock.Setup(s => s.GetProviderName()).Returns("CurrencyLayer");

        module.SetStrategy(layerMock.Object);
        var resultAfter = await module.ProcessInvoice(10m, "ZAR");
        Assert.Contains("CurrencyLayer", resultAfter);
    }

    [Fact]
    public async Task FinancialIntegrationModule_ProcessInvoice_NegativeAmount_Throws()
    {
        var strategy = new Mock<ICurrencyStrategy>();
        var module   = new FinancialIntegrationModule(strategy.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => module.ProcessInvoice(-1m, "ZAR"));
    }
}
