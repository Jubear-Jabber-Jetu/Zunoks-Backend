using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class ScreeningSurveySection
{
    [Key]
    public int Id { get; set; }

    public int SurveyId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PartLabel { get; set; } = string.Empty;

    /// <summary>Technical, Leadership, Tool, or General</summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsScored { get; set; }

    [ForeignKey(nameof(SurveyId))]
    public virtual ScreeningSurvey Survey { get; set; } = null!;

    public virtual ICollection<ScreeningSurveyQuestion> Questions { get; set; } = new List<ScreeningSurveyQuestion>();

    public virtual ICollection<ScreeningSectionScore> SectionScores { get; set; } = new List<ScreeningSectionScore>();
}
