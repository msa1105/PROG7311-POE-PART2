using PROG7311_POE.Models;

namespace PROG7311_POE.Services;

public class ContractWorkflowService : IContractWorkflowService
{
    //Code attribution
    //Microsoft. 2023. Model Validation in ASP.NET Core.
    //Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
    //[Accessed: 15 January 2025]
    //
    //Code attribution
    //Geeks for Geeks. 2023. Software Design Patterns - Business Rules.
    //Available at: https://www.geeksforgeeks.org/software-engineering-design-patterns/
    //[Accessed: 15 January 2025]
    public void ValidateServiceRequestCreation(Contract contract)
    {
        // Business Rule: Block service request creation for Expired and OnHold contracts
        // Draft contracts can have service requests created (they're being prepared)
        if (contract.Status == ContractStatus.Expired || 
            contract.Status == ContractStatus.OnHold)
        {
            throw new InvalidOperationException(
                $"Cannot create a Service Request for a contract with status '{contract.Status}'. " +
                "The contract must be Active or Draft.");
        }
    }
}
