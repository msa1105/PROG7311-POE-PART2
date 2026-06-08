//Code attribution
//Refactoring.Guru. 2024. Observer Design Pattern.
//Available at: https://refactoring.guru/design-patterns/observer
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Observer;

/// <summary>
/// Concrete Observer — Compliance Observer.
/// Logs a compliance violation whenever a contract transitions into a restricted status.
/// </summary>
public class ComplianceObserver : IObserver
{
    /// <summary>Append-only log of all compliance events recorded by this observer.</summary>
    public IReadOnlyList<string> ComplianceLog => _complianceLog.AsReadOnly();
    private readonly List<string> _complianceLog = new();

    private static readonly HashSet<ContractStatus> RestrictedStatuses =
        new() { ContractStatus.Expired, ContractStatus.OnHold };

    public void Update(ContractStatus status)
    {
        if (RestrictedStatuses.Contains(status))
            LogViolation(status);
    }

    /// <summary>Records a compliance violation entry for the given restricted status.</summary>
    public void LogViolation(ContractStatus status)
    {
        var entry = $"[COMPLIANCE] Violation recorded: Contract moved to '{status}' at {DateTime.UtcNow:yy/MM/dd HH:mm}. Review required.";
        _complianceLog.Add(entry);
    }
}
