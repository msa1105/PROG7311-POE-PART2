//Code attribution
//Refactoring.Guru. 2024. Strategy Design Pattern.
//Available at: https://refactoring.guru/design-patterns/strategy
//[Accessed: 22 April 2025]

namespace PROG7311_POE.Patterns.Strategy;

/// <summary>
/// Strategy Pattern — Strategy Interface.
/// Defines the contract for interchangeable currency conversion implementations.
/// </summary>
public interface ICurrencyStrategy
{
    /// <summary>
    /// Converts an <paramref name="amount"/> from one ISO 4217 currency to another.
    /// </summary>
    Task<decimal> Convert(decimal amount, string from, string to);

    /// <summary>Returns the human-readable name of this conversion provider.</summary>
    string GetProviderName();
}
