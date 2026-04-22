//Code attribution
//Refactoring.Guru. 2024. Factory Method Design Pattern.
//Available at: https://refactoring.guru/design-patterns/factory-method
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Factory;

/// <summary>
/// Concrete Product — Domestic Contract.
/// Represents a contract scoped to a domestic (local) region.
/// </summary>
public class DomesticContract : IContract
{
    public int ContractId { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    /// <summary>Geographic region for this domestic contract (e.g. "Gauteng").</summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>Applicable local billing rate in ZAR per hour.</summary>
    public decimal LocalRate { get; set; }

    public bool Validate()
    {
        if (ContractId <= 0)
            throw new InvalidOperationException("ContractId must be a positive integer.");
        if (string.IsNullOrWhiteSpace(Region))
            throw new InvalidOperationException("Region is required for a Domestic Contract.");
        if (LocalRate <= 0)
            throw new InvalidOperationException("LocalRate must be greater than zero.");
        return true;
    }

    public string GetDetails() =>
        $"[Domestic] Contract #{ContractId} | Region: {Region} | Rate: {LocalRate:C} ZAR/hr | Status: {Status}";
}
