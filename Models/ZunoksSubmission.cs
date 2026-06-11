using System.ComponentModel.DataAnnotations;

namespace ZunoksBackend.Models;

public class ZunoksSubmission
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string CompanyName { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public virtual ICollection<ZunoksResponse> Responses { get; set; } = new List<ZunoksResponse>();
    public virtual ICollection<SelectedModule> SelectedModules { get; set; } = new List<SelectedModule>();
}
