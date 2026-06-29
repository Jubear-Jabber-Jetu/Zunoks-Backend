using System.ComponentModel.DataAnnotations;

namespace ZunoksBackend.Models;

public class ReComLeadService
{
    [Key]
    public int Id { get; set; }

    public int ReComLeadId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    public virtual ReComLead ReComLead { get; set; } = null!;
}
