using System.ComponentModel.DataAnnotations;

namespace ZunoksBackend.Models;

public class ScreeningSurvey
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ScreeningSurveySection> Sections { get; set; } = new List<ScreeningSurveySection>();

    public virtual ICollection<ScreeningSubmission> Submissions { get; set; } = new List<ScreeningSubmission>();
}
