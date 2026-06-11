using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class SelectedModule
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ZunoksSubmissionId { get; set; }

    [ForeignKey("ZunoksSubmissionId")]
    public virtual ZunoksSubmission ZunoksSubmission { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string ModuleName { get; set; } = string.Empty;
}
