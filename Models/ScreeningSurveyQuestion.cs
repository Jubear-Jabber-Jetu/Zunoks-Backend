using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class ScreeningSurveyQuestion
{
    [Key]
    public int Id { get; set; }

    public int SectionId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Radio or Text</summary>
    [Required]
    [MaxLength(20)]
    public string QuestionType { get; set; } = "Radio";

    public int SortOrder { get; set; }

    public bool IsRequired { get; set; }

    public bool IsScored { get; set; }

    [ForeignKey(nameof(SectionId))]
    public virtual ScreeningSurveySection Section { get; set; } = null!;

    public virtual ICollection<ScreeningSurveyOption> Options { get; set; } = new List<ScreeningSurveyOption>();

    public virtual ICollection<ScreeningAnswer> Answers { get; set; } = new List<ScreeningAnswer>();
}
