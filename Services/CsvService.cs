using System.Text;
using ZunoksBackend.Models;

namespace ZunoksBackend.Services;

public class CsvService : ICsvService
{
    public byte[] GenerateLongCsv(IEnumerable<ZunoksSubmission> submissions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SubmissionId,CompanyName,SubmittedAt,Module,QuestionId,QuestionLabel,Answer");

        foreach (var submission in submissions.OrderByDescending(s => s.SubmittedAt))
        {
            foreach (var response in submission.Responses.OrderBy(r => r.Module).ThenBy(r => r.QuestionId))
            {
                sb.AppendLine(string.Join(",",
                    submission.Id.ToString(),
                    CsvEscape(submission.CompanyName),
                    CsvEscape(submission.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    CsvEscape(response.Module),
                    CsvEscape(response.QuestionId),
                    CsvEscape(response.QuestionLabel ?? response.QuestionId),
                    CsvEscape(ExportFormatHelper.FormatAnswer(response.Answer))));
            }
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(sb.ToString());
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
