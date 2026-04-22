using System.ComponentModel.DataAnnotations;

namespace PROG7311_POE.Models;

public class Client
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Client Name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(500)]
    [Display(Name = "Contact Details")]
    public string ContactDetails { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
