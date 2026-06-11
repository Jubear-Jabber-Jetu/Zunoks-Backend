namespace ZunoksBackend.Models.DTOs;

public class ZunoksSubmissionDto
{
    public string CompanyName { get; set; } = string.Empty;
    public List<string> SelectedModules { get; set; } = new();
    public Dictionary<string, Dictionary<string, string>> Responses { get; set; } = new();
    public Dictionary<string, Dictionary<string, string>>? QuestionLabels { get; set; }
}
