using System.ComponentModel.DataAnnotations;

namespace ZunoksBackend.Models;

public class ReComLead
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Company { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string CompanySize { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(4000)]
    public string? Details { get; set; }

    public DateTime SubmittedAt { get; set; }

    public virtual ICollection<ReComLeadService> Services { get; set; } = new List<ReComLeadService>();
}
