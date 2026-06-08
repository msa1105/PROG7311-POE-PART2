//Code attribution
//Refactoring.Guru. 2024. Observer Design Pattern.
//Available at: https://refactoring.guru/design-patterns/observer
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Observer;

/// <summary>
/// Observer Pattern — Subject.
/// Maintains a list of observers and notifies them whenever the contract status changes.
/// </summary>
public class ContractSubject
{
    private readonly List<IObserver> _observers = new();
    private ContractStatus _currentStatus;

    public ContractStatus CurrentStatus => _currentStatus;

    /// <summary>Registers an observer to receive status-change notifications.</summary>
    public void Attach(IObserver observer)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    /// <summary>Removes a previously registered observer.</summary>
    public void Detach(IObserver observer)
    {
        _observers.Remove(observer);
    }

    /// <summary>Pushes the current status to all registered observers.</summary>
    public void Notify()
    {
        foreach (var observer in _observers)
            observer.Update(_currentStatus);
    }

    /// <summary>
    /// Updates the contract status and immediately notifies all observers.
    /// </summary>
    public void SetStatus(ContractStatus newStatus)
    {
        _currentStatus = newStatus;
        Notify();
    }
}
