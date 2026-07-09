using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class ScreeningSectionScore
{
    [Key]
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    public int SectionId { get; set; }

    public int Score { get; set; }

    public int MaxScore { get; set; }

    [ForeignKey(nameof(SubmissionId))]
    public virtual ScreeningSubmission Submission { get; set; } = null!;

    [ForeignKey(nameof(SectionId))]
    public virtual ScreeningSurveySection Section { get; set; } = null!;
}
