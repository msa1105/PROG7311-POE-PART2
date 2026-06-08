//Code attribution
//Refactoring.Guru. 2024. Observer Design Pattern.
//Available at: https://refactoring.guru/design-patterns/observer
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Observer;

/// <summary>
/// Concrete Observer — Logistics Manager.
/// Sends an alert to the responsible logistics manager when a contract status changes.
/// </summary>
public class LogisticsManagerObserver : IObserver
{
    /// <summary>Unique identifier for the logistics manager receiving alerts.</summary>
    public string ManagerId { get; }

    /// <summary>Log of all alerts dispatched by this observer.</summary>
    public IReadOnlyList<string> AlertLog => _alertLog.AsReadOnly();
    private readonly List<string> _alertLog = new();

    public LogisticsManagerObserver(string managerId)
    {
        if (string.IsNullOrWhiteSpace(managerId))
            throw new ArgumentException("ManagerId cannot be empty.", nameof(managerId));
        ManagerId = managerId;
    }

    public void Update(ContractStatus status)
    {
        var message = SendAlert(status);
        _alertLog.Add(message);
    }

    /// <summary>Composes and stores an alert message for the given status change.</summary>
    public string SendAlert(ContractStatus status)
    {
        var message = $"[ALERT] Manager {ManagerId}: Contract status changed to '{status}' at {DateTime.UtcNow:yy/MM/dd HH:mm}.";
        return message;
    }
}
