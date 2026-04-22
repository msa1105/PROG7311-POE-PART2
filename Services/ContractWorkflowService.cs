using PROG7311_POE.Models;

namespace PROG7311_POE.Services;

public class ContractWorkflowService : IContractWorkflowService
{
    public void ValidateServiceRequestCreation(Contract contract)
    {
        if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
            throw new InvalidOperationException(
                $"Cannot create a Service Request for a contract with status '{contract.Status}'. " +
                "The contract must be Active or in Draft status.");
    }
}
