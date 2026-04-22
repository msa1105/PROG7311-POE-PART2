namespace PROG7311_POE.Models.ViewModels;

public class ContractFilterViewModel
{
    public string? StartDateFrom { get; set; }
    public string? StartDateTo { get; set; }
    public string? EndDateFrom { get; set; }
    public string? EndDateTo { get; set; }
    public ContractStatus? Status { get; set; }
    public string? ClientName { get; set; }
    public List<Contract> Results { get; set; } = new();
}
