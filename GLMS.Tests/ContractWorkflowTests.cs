using PROG7311_POE.Models;
using PROG7311_POE.Services;

namespace GLMS.Tests;

/// <summary>
/// Test 3 – Verify business workflow: ServiceRequest creation must fail
/// when the parent Contract is Expired or OnHold.
/// </summary>
public class ContractWorkflowTests
{
    private readonly ContractWorkflowService _sut = new();

    [Theory]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public void ValidateServiceRequestCreation_ExpiredOrOnHold_ThrowsInvalidOperationException(
        ContractStatus status)
    {
        var contract = new Contract
        {
            Id = 1,
            ClientId = 1,
            StartDate = DateTime.Today.AddYears(-2),
            EndDate = DateTime.Today.AddYears(-1),
            Status = status
        };

        Assert.Throws<InvalidOperationException>(
            () => _sut.ValidateServiceRequestCreation(contract));
    }

    [Theory]
    [InlineData(ContractStatus.Active)]
    [InlineData(ContractStatus.Draft)]
    public void ValidateServiceRequestCreation_ActiveOrDraft_DoesNotThrow(
        ContractStatus status)
    {
        var contract = new Contract
        {
            Id = 2,
            ClientId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            Status = status
        };

        // Should not throw
        var ex = Record.Exception(() => _sut.ValidateServiceRequestCreation(contract));
        Assert.Null(ex);
    }
}
