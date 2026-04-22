using PROG7311_POE.Models;

namespace PROG7311_POE.Services;

public class ContractWorkflowService : IContractWorkflowService
{
    //Code attribution
    //OpenAI. 2025. ChatGPT (Version GPT-4). [Large language model]
    //Available at: https://chat.openai.com/
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
