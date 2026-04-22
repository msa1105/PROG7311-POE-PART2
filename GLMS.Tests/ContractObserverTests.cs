using PROG7311_POE.Models;
using PROG7311_POE.Patterns.Observer;

namespace GLMS.Tests;

/// <summary>
/// Tests for the Observer Pattern (Status Tracking).
/// Verifies that ContractSubject notifies observers and that concrete observers
/// respond correctly to status changes.
/// </summary>
public class ContractObserverTests
{
    [Fact]
    public void ContractSubject_SetStatus_NotifiesAllObservers()
    {
        var subject  = new ContractSubject();
        var manager  = new LogisticsManagerObserver("MGR-001");
        var compliance = new ComplianceObserver();

        subject.Attach(manager);
        subject.Attach(compliance);
        subject.SetStatus(ContractStatus.Active);

        Assert.Equal(ContractStatus.Active, subject.CurrentStatus);
        Assert.Single(manager.AlertLog);
        Assert.Empty(compliance.ComplianceLog); // Active is not a restricted status
    }

    [Fact]
    public void ContractSubject_SetStatus_Expired_ComplianceLogsViolation()
    {
        var subject    = new ContractSubject();
        var compliance = new ComplianceObserver();
        subject.Attach(compliance);

        subject.SetStatus(ContractStatus.Expired);

        Assert.Single(compliance.ComplianceLog);
        Assert.Contains("Expired", compliance.ComplianceLog[0]);
    }

    [Fact]
    public void ContractSubject_SetStatus_OnHold_ComplianceLogsViolation()
    {
        var subject    = new ContractSubject();
        var compliance = new ComplianceObserver();
        subject.Attach(compliance);

        subject.SetStatus(ContractStatus.OnHold);

        Assert.Single(compliance.ComplianceLog);
        Assert.Contains("OnHold", compliance.ComplianceLog[0]);
    }

    [Fact]
    public void ContractSubject_Detach_StopsReceivingNotifications()
    {
        var subject = new ContractSubject();
        var manager = new LogisticsManagerObserver("MGR-002");

        subject.Attach(manager);
        subject.SetStatus(ContractStatus.Active);
        subject.Detach(manager);
        subject.SetStatus(ContractStatus.Expired);

        // Only the first notification was received
        Assert.Single(manager.AlertLog);
    }

    [Fact]
    public void LogisticsManagerObserver_SendAlert_ContainsManagerId()
    {
        var manager = new LogisticsManagerObserver("MGR-003");
        var alert   = manager.SendAlert(ContractStatus.OnHold);
        Assert.Contains("MGR-003", alert);
    }

    [Fact]
    public void ContractSubject_MultipleStatusChanges_AllObserversReceiveAll()
    {
        var subject = new ContractSubject();
        var manager = new LogisticsManagerObserver("MGR-004");
        subject.Attach(manager);

        subject.SetStatus(ContractStatus.Draft);
        subject.SetStatus(ContractStatus.Active);
        subject.SetStatus(ContractStatus.Expired);

        Assert.Equal(3, manager.AlertLog.Count);
    }
}
