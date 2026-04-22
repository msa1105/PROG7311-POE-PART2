using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG7311_POE.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Cost (USD)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than zero.")]
    public decimal Cost { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Local Cost (ZAR)")]
    public decimal LocalCost { get; set; }

    [StringLength(100)]
    public string? Status { get; set; }

    public Contract? Contract { get; set; }
}
