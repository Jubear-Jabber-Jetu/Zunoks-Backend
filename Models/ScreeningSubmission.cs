using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class ScreeningSubmission
{
    [Key]
    public int Id { get; set; }

    public int SurveyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? YearsOfExperience { get; set; }

    [MaxLength(200)]
    public string? NoticePeriod { get; set; }

    [MaxLength(2000)]
    public string? PortfolioLinks { get; set; }

    [MaxLength(100)]
    public string? ExpectedSalary { get; set; }

    public DateTime SubmittedAt { get; set; }

    public int TotalScore { get; set; }

    public int MaxScore { get; set; }

    public decimal Percentage { get; set; }

    public bool IsCompleted { get; set; }

    [ForeignKey(nameof(SurveyId))]
    public virtual ScreeningSurvey Survey { get; set; } = null!;

    public virtual ICollection<ScreeningAnswer> Answers { get; set; } = new List<ScreeningAnswer>();

    public virtual ICollection<ScreeningSectionScore> SectionScores { get; set; } = new List<ScreeningSectionScore>();
}
