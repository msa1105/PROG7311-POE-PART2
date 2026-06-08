//Code attribution
//Refactoring.Guru. 2024. Observer Design Pattern.
//Available at: https://refactoring.guru/design-patterns/observer
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Observer;

/// <summary>
/// Observer Pattern — Observer Interface.
/// All status-tracking observers must implement this interface.
/// </summary>
public interface IObserver
{
    /// <summary>Called by the subject when the contract status changes.</summary>
    void Update(ContractStatus status);
}
