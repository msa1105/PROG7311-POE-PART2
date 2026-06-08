//Code attribution
//Refactoring.Guru. 2024. Strategy Design Pattern.
//Available at: https://refactoring.guru/design-patterns/strategy
//[Accessed: 22 April 2025]

namespace PROG7311_POE.Patterns.Strategy;

/// <summary>
/// Strategy Pattern — Context.
/// The FinancialIntegrationModule holds a reference to an ICurrencyStrategy
/// and delegates all conversion work to the active strategy.
/// Strategies can be swapped at runtime via SetStrategy().
/// </summary>
public class FinancialIntegrationModule
{
    private ICurrencyStrategy _strategy;

    public FinancialIntegrationModule(ICurrencyStrategy initialStrategy)
    {
        _strategy = initialStrategy ?? throw new ArgumentNullException(nameof(initialStrategy));
    }

    /// <summary>Swaps the active currency conversion strategy at runtime.</summary>
    public void SetStrategy(ICurrencyStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <summary>
    /// Converts the invoice amount from USD to the target currency using the active strategy.
    /// Returns a formatted invoice summary string.
    /// </summary>
    public async Task<string> ProcessInvoice(decimal usdAmount, string targetCurrency)
    {
        if (usdAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(usdAmount), "Invoice amount must be positive.");
        if (string.IsNullOrWhiteSpace(targetCurrency))
            throw new ArgumentException("Target currency is required.", nameof(targetCurrency));

        var converted = await ExecuteConversion(usdAmount, "USD", targetCurrency);
        return $"Invoice: {usdAmount:F2} USD = {converted:F2} {targetCurrency.ToUpperInvariant()} (via {_strategy.GetProviderName()})";
    }

    /// <summary>
    /// Executes the raw currency conversion using the active strategy.
    /// </summary>
    public Task<decimal> ExecuteConversion(decimal amount, string from, string to)
        => _strategy.Convert(amount, from, to);
}
