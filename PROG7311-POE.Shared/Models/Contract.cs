using System.ComponentModel.DataAnnotations;

namespace PROG7311_POE.Models;

public class Contract
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yy/MM/dd}", ApplyFormatInEditMode = true)]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yy/MM/dd}", ApplyFormatInEditMode = true)]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Display(Name = "Service Level")]
    [StringLength(200)]
    public string? ServiceLevel { get; set; }

    [Display(Name = "Agreement File")]
    public string? AgreementFilePath { get; set; }

    public Client? Client { get; set; }
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
