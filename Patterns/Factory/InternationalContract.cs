//Code attribution
//Refactoring.Guru. 2024. Factory Method Design Pattern.
//Available at: https://refactoring.guru/design-patterns/factory-method
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Factory;

/// <summary>
/// Concrete Product — International Contract.
/// Represents a cross-border contract with currency and country metadata.
/// </summary>
public class InternationalContract : IContract
{
    public int ContractId { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    /// <summary>ISO 3166-1 alpha-2 country code for the shipment origin (e.g. "ZA").</summary>
    public string OriginCountry { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 country code for the shipment destination (e.g. "US").</summary>
    public string DestinationCountry { get; set; } = string.Empty;

    /// <summary>ISO 4217 currency code for invoicing (e.g. "USD").</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    public bool Validate()
    {
        if (ContractId <= 0)
            throw new InvalidOperationException("ContractId must be a positive integer.");
        if (string.IsNullOrWhiteSpace(OriginCountry))
            throw new InvalidOperationException("OriginCountry is required for an International Contract.");
        if (string.IsNullOrWhiteSpace(DestinationCountry))
            throw new InvalidOperationException("DestinationCountry is required for an International Contract.");
        if (string.IsNullOrWhiteSpace(CurrencyCode))
            throw new InvalidOperationException("CurrencyCode is required for an International Contract.");
        return true;
    }

    public string GetDetails() =>
        $"[International] Contract #{ContractId} | {OriginCountry} -> {DestinationCountry} | Currency: {CurrencyCode} | Status: {Status}";
}
