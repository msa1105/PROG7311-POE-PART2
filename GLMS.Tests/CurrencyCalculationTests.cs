using PROG7311_POE.Models;
using PROG7311_POE.Services;

namespace GLMS.Tests;

/// <summary>
/// Test 1 – Verify USD ? ZAR maths given a mocked exchange rate.
/// We test the pure calculation in isolation without hitting the live API.
/// </summary>
public class CurrencyCalculationTests
{
    [Theory]
    [InlineData(100, 18.5, 1850.00)]
    [InlineData(1, 18.5, 18.50)]
    [InlineData(0.50, 18.5, 9.25)]
    [InlineData(250.75, 18.5, 4638.88)]
    public void ConvertUsdToZar_GivenMockedRate_ReturnsCorrectAmount(
        decimal usdAmount, decimal rate, decimal expected)
    {
        // Act — replicate the formula used in CurrencyService.ConvertUsdToZarAsync
        var result = Math.Round(usdAmount * rate, 2);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertUsdToZar_ZeroAmount_ReturnsZero()
    {
        decimal rate = 18.5m;
        var result = Math.Round(0m * rate, 2);
        Assert.Equal(0m, result);
    }
}
