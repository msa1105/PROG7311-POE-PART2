using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PROG7311_POE.Models.ViewModels;

// Code attribution
// Microsoft. 2023. Model Validation in ASP.NET Core.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
// [Accessed: 15 January 2025]
//
// Code attribution
// Stack Overflow. 2014. IValidatableObject cross-property validation example.
// Available at: https://stackoverflow.com/questions/3400542/how-do-i-use-ivalidatableobject
// [Accessed: 15 January 2025]
public class ContractFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a client.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid client.")]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yy/MM/dd}", ApplyFormatInEditMode = true)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "End date is required.")]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yy/MM/dd}", ApplyFormatInEditMode = true)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [StringLength(200, ErrorMessage = "Service level cannot exceed 200 characters.")]
    [Display(Name = "Service Level")]
    public string? ServiceLevel { get; set; }

    [Display(Name = "Signed Agreement (PDF only, max 10 MB)")]
    public IFormFile? AgreementFile { get; set; }

    public string? ExistingAgreementFilePath { get; set; }

    // Cross-field validation: end date must be after start date
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult(
                "End Date must be after Start Date.",
                new[] { nameof(EndDate) });

        if (StartDate < new DateTime(2000, 1, 1))
            yield return new ValidationResult(
                "Start Date seems too far in the past. Please check the date.",
                new[] { nameof(StartDate) });

        if (AgreementFile != null)
        {
            const long maxBytes = 10 * 1024 * 1024; // 10 MB
            if (AgreementFile.Length > maxBytes)
                yield return new ValidationResult(
                    "The uploaded file exceeds the 10 MB size limit.",
                    new[] { nameof(AgreementFile) });
        }
    }
}
