//Code attribution
//Refactoring.Guru. 2024. Factory Method Design Pattern.
//Available at: https://refactoring.guru/design-patterns/factory-method
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Factory;

/// <summary>
/// Factory Pattern — Product Interface.
/// Defines the contract that all concrete contract types must implement.
/// </summary>
public interface IContract
{
    int ContractId { get; set; }
    ContractStatus Status { get; set; }

    /// <summary>Validates that the contract's required fields are correctly populated.</summary>
    bool Validate();

    /// <summary>Returns a human-readable summary of the contract details.</summary>
    string GetDetails();
}
