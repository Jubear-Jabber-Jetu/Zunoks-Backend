using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using ZunoksBackend.Models;

namespace ZunoksBackend.Services;

public class ScreeningExportService : IScreeningExportService
{
    public byte[] GenerateExcel(IEnumerable<ScreeningSubmission> submissions)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var list = submissions.OrderByDescending(s => s.SubmittedAt).ToList();

        using var package = new ExcelPackage();

        var summary = package.Workbook.Worksheets.Add("Summary");
        var headers = new[]
        {
            "ID", "Full Name", "Email", "Phone", "Experience", "Notice Period",
            "Expected Salary", "Portfolio Links", "Submitted At",
            "Total Score", "Max Score", "Percentage",
            "Technical Score", "Leadership Score", "Tool Score",
        };

        for (var col = 1; col <= headers.Length; col++)
        {
            summary.Cells[1, col].Value = headers[col - 1];
            StyleHeader(summary.Cells[1, col]);
        }

        var row = 2;
        foreach (var s in list)
        {
            summary.Cells[row, 1].Value = s.Id;
            summary.Cells[row, 2].Value = s.FullName;
            summary.Cells[row, 3].Value = s.Email;
            summary.Cells[row, 4].Value = s.Phone;
            summary.Cells[row, 5].Value = s.YearsOfExperience;
            summary.Cells[row, 6].Value = s.NoticePeriod;
            summary.Cells[row, 7].Value = s.ExpectedSalary;
            summary.Cells[row, 8].Value = s.PortfolioLinks;
            summary.Cells[row, 9].Value = s.SubmittedAt.ToString("yyyy-MM-dd HH:mm");
            summary.Cells[row, 10].Value = s.TotalScore;
            summary.Cells[row, 11].Value = s.MaxScore;
            summary.Cells[row, 12].Value = s.Percentage;
            summary.Cells[row, 13].Value = SumCategory(s, "Technical");
            summary.Cells[row, 14].Value = SumCategory(s, "Leadership");
            summary.Cells[row, 15].Value = SumCategory(s, "Tool");
            row++;
        }

        summary.Cells[summary.Dimension!.Address].AutoFitColumns();

        var details = package.Workbook.Worksheets.Add("Answers");
        var detailHeaders = new[]
        {
            "Submission ID", "Candidate", "Email", "Section", "Question", "Answer", "Score",
        };

        for (var col = 1; col <= detailHeaders.Length; col++)
        {
            details.Cells[1, col].Value = detailHeaders[col - 1];
            StyleHeader(details.Cells[1, col]);
        }

        row = 2;
        foreach (var s in list)
        {
            foreach (var answer in s.Answers.OrderBy(a => a.Question.Section.SortOrder).ThenBy(a => a.Question.SortOrder))
            {
                details.Cells[row, 1].Value = s.Id;
                details.Cells[row, 2].Value = s.FullName;
                details.Cells[row, 3].Value = s.Email;
                details.Cells[row, 4].Value = answer.Question.Section.Title;
                details.Cells[row, 5].Value = answer.Question.Text;
                details.Cells[row, 6].Value = answer.Option?.Label ?? answer.TextAnswer;
                details.Cells[row, 7].Value = answer.ScoreEarned;
                row++;
            }
        }

        details.Cells[details.Dimension!.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] GenerateCsv(IEnumerable<ScreeningSubmission> submissions)
    {
        var list = submissions.OrderByDescending(s => s.SubmittedAt).ToList();
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(",", new[]
        {
            "SubmissionId", "FullName", "Email", "Phone", "Experience", "NoticePeriod",
            "ExpectedSalary", "PortfolioLinks", "SubmittedAt", "Section", "Question",
            "Answer", "ScoreEarned", "TotalScore", "MaxScore", "Percentage",
        }));

        foreach (var s in list)
        {
            foreach (var answer in s.Answers.OrderBy(a => a.Question.Section.SortOrder).ThenBy(a => a.Question.SortOrder))
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Csv(s.Id.ToString()),
                    Csv(s.FullName),
                    Csv(s.Email),
                    Csv(s.Phone),
                    Csv(s.YearsOfExperience),
                    Csv(s.NoticePeriod),
                    Csv(s.ExpectedSalary),
                    Csv(s.PortfolioLinks),
                    Csv(s.SubmittedAt.ToString("yyyy-MM-dd HH:mm")),
                    Csv(answer.Question.Section.Title),
                    Csv(answer.Question.Text),
                    Csv(answer.Option?.Label ?? answer.TextAnswer),
                    Csv(answer.ScoreEarned.ToString()),
                    Csv(s.TotalScore.ToString()),
                    Csv(s.MaxScore.ToString()),
                    Csv(s.Percentage.ToString()),
                }));
            }
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static int SumCategory(ScreeningSubmission s, string category)
    {
        return s.SectionScores
            .Where(ss => string.Equals(ss.Section.Category, category, StringComparison.OrdinalIgnoreCase))
            .Sum(ss => ss.Score);
    }

    private static string Csv(string? value)
    {
        var v = value ?? string.Empty;
        if (v.Contains('"') || v.Contains(',') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }

    private static void StyleHeader(ExcelRange cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 70, 229));
        cell.Style.Font.Color.SetColor(Color.White);
    }
}
