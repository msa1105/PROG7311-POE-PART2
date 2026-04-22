using PROG7311_POE.Models;

namespace PROG7311_POE.Services;

public interface IContractWorkflowService
{
    void ValidateServiceRequestCreation(Contract contract);
}
