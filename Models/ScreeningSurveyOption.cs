using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZunoksBackend.Models;

public class ScreeningSurveyOption
{
    [Key]
    public int Id { get; set; }

    public int QuestionId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Label { get; set; } = string.Empty;

    public int Score { get; set; }

    public int SortOrder { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public virtual ScreeningSurveyQuestion Question { get; set; } = null!;

    public virtual ICollection<ScreeningAnswer> Answers { get; set; } = new List<ScreeningAnswer>();
}
