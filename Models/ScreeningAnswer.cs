using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class ScreeningAnswer
{
    [Key]
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    public int QuestionId { get; set; }

    public int? OptionId { get; set; }

    [MaxLength(4000)]
    public string? TextAnswer { get; set; }

    public int ScoreEarned { get; set; }

    [ForeignKey(nameof(SubmissionId))]
    public virtual ScreeningSubmission Submission { get; set; } = null!;

    [ForeignKey(nameof(QuestionId))]
    public virtual ScreeningSurveyQuestion Question { get; set; } = null!;

    [ForeignKey(nameof(OptionId))]
    public virtual ScreeningSurveyOption? Option { get; set; }
}
