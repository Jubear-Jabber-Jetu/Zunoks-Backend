using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class ZunoksResponse
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ZunoksSubmissionId { get; set; }

    [ForeignKey("ZunoksSubmissionId")]
    public virtual ZunoksSubmission ZunoksSubmission { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Module { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string QuestionId { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? QuestionLabel { get; set; }

    [MaxLength(4000)]
    public string Answer { get; set; } = string.Empty;
}
