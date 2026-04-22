using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PROG7311_POE.Models.ViewModels;

public class ContractFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yy/MM/dd}", ApplyFormatInEditMode = true)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yy/MM/dd}", ApplyFormatInEditMode = true)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Display(Name = "Service Level")]
    public string? ServiceLevel { get; set; }

    [Display(Name = "Signed Agreement (PDF only)")]
    public IFormFile? AgreementFile { get; set; }

    public string? ExistingAgreementFilePath { get; set; }
}
