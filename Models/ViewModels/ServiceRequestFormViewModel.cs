using System.ComponentModel.DataAnnotations;

namespace PROG7311_POE.Models.ViewModels;

public class ServiceRequestFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Cost (USD)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than zero.")]
    public decimal Cost { get; set; }

    [StringLength(100)]
    public string? Status { get; set; }
}
