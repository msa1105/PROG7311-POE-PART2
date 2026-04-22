using System.ComponentModel.DataAnnotations;

namespace PROG7311_POE.Models.ViewModels;

// Code attribution
// Microsoft. 2023. Model Validation in ASP.NET Core.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
// [Accessed: 15 January 2025]
public class ServiceRequestFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a contract.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid contract.")]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cost is required.")]
    [Display(Name = "Cost (USD)")]
    [Range(0.01, 1_000_000, ErrorMessage = "Cost must be between $0.01 and $1,000,000.")]
    public decimal Cost { get; set; }

    [StringLength(100, ErrorMessage = "Status cannot exceed 100 characters.")]
    public string? Status { get; set; }
}
