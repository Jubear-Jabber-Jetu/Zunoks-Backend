using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using ZunoksBackend.Models;

namespace ZunoksBackend.Services;

public class ExcelService : IExcelService
{
    public byte[] GenerateExcel(ZunoksSubmission submission)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Zunoks Responses");

        int row = 1;

        worksheet.Cells[row, 1].Value = "Organisation Name";
        worksheet.Cells[row, 2].Value = "Module";
        worksheet.Cells[row, 3].Value = "Question";
        worksheet.Cells[row, 4].Value = "Answer";

        StyleHeader(worksheet.Cells[row, 1, row, 4]);
        row++;

        worksheet.Cells[row, 1].Value = submission.CompanyName;
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        row++;

        worksheet.Cells[row, 1].Value = "Completed Modules";
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        row++;

        foreach (var module in submission.SelectedModules)
        {
            worksheet.Cells[row, 2].Value = module.ModuleName;
            row++;
        }

        row++;

        worksheet.Cells[row, 1].Value = "Survey Responses";
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        row++;

        foreach (var response in submission.Responses.OrderBy(r => r.Module).ThenBy(r => r.QuestionId))
        {
            worksheet.Cells[row, 2].Value = response.Module;
            worksheet.Cells[row, 3].Value = string.IsNullOrWhiteSpace(response.QuestionLabel)
                ? response.QuestionId
                : response.QuestionLabel;
            worksheet.Cells[row, 4].Value = ExportFormatHelper.FormatAnswer(response.Answer);
            row++;
        }

        worksheet.Cells[worksheet.Dimension!.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] GenerateMasterWorkbook(IEnumerable<ZunoksSubmission> submissions)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var list = submissions.OrderByDescending(s => s.SubmittedAt).ToList();

        using var package = new ExcelPackage();
        BuildIndexSheet(package, list);
        BuildWideSheet(package, list);
        BuildLongSheet(package, list);
        BuildCodebookSheet(package, list);
        return package.GetAsByteArray();
    }

    public byte[] GenerateCompareMatrixExcel(IEnumerable<ZunoksSubmission> submissions, string moduleName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var companies = submissions
            .Where(s => s.SelectedModules.Any(m => m.ModuleName == moduleName))
            .OrderBy(s => s.CompanyName)
            .ToList();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add(SafeSheetName(moduleName));

        var questions = companies
            .SelectMany(s => s.Responses.Where(r => r.Module == moduleName))
            .Select(r => string.IsNullOrWhiteSpace(r.QuestionLabel) ? r.QuestionId : r.QuestionLabel!)
            .Distinct()
            .OrderBy(q => q)
            .ToList();

        ws.Cells[1, 1].Value = "Question / Organisation";
        StyleHeader(ws.Cells[1, 1, 1, companies.Count + 1]);

        for (var i = 0; i < companies.Count; i++)
            ws.Cells[1, i + 2].Value = companies[i].CompanyName;

        for (var q = 0; q < questions.Count; q++)
        {
            var row = q + 2;
            ws.Cells[row, 1].Value = questions[q];
            ws.Cells[row, 1].Style.Font.Bold = true;

            for (var c = 0; c < companies.Count; c++)
            {
                var answer = companies[c].Responses.FirstOrDefault(r =>
                    r.Module == moduleName &&
                    (r.QuestionLabel == questions[q] || r.QuestionId == questions[q]));
                ws.Cells[row, c + 2].Value = answer != null
                    ? ExportFormatHelper.FormatAnswer(answer.Answer)
                    : "-";
            }
        }

        ws.Cells[ws.Dimension!.Address].AutoFitColumns();
        ws.View.FreezePanes(2, 2);
        return package.GetAsByteArray();
    }

    private static void BuildIndexSheet(ExcelPackage package, List<ZunoksSubmission> submissions)
    {
        var ws = package.Workbook.Worksheets.Add("Index");
        ws.Cells[1, 1].Value = "SubmissionId";
        ws.Cells[1, 2].Value = "CompanyName";
        ws.Cells[1, 3].Value = "SubmittedAt";
        ws.Cells[1, 4].Value = "ModuleCount";
        ws.Cells[1, 5].Value = "Modules";
        StyleHeader(ws.Cells[1, 1, 1, 5]);

        for (var i = 0; i < submissions.Count; i++)
        {
            var row = i + 2;
            var s = submissions[i];
            ws.Cells[row, 1].Value = s.Id;
            ws.Cells[row, 2].Value = s.CompanyName;
            ws.Cells[row, 3].Value = s.SubmittedAt;
            ws.Cells[row, 3].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
            ws.Cells[row, 4].Value = s.SelectedModules.Count;
            ws.Cells[row, 5].Value = string.Join("; ", s.SelectedModules.Select(m => m.ModuleName));
        }

        ws.Cells[ws.Dimension!.Address].AutoFitColumns();
    }

    private static void BuildWideSheet(ExcelPackage package, List<ZunoksSubmission> submissions)
    {
        var ws = package.Workbook.Worksheets.Add("Wide Analysis");
        var questionColumns = submissions
            .SelectMany(s => s.Responses)
            .Select(r => ExportFormatHelper.ColumnKey(r.Module, r.QuestionId))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        ws.Cells[1, 1].Value = "SubmissionId";
        ws.Cells[1, 2].Value = "CompanyName";
        ws.Cells[1, 3].Value = "SubmittedAt";
        StyleHeader(ws.Cells[1, 1, 1, 3 + questionColumns.Count]);

        for (var i = 0; i < questionColumns.Count; i++)
            ws.Cells[1, i + 4].Value = questionColumns[i];

        for (var s = 0; s < submissions.Count; s++)
        {
            var row = s + 2;
            var submission = submissions[s];
            ws.Cells[row, 1].Value = submission.Id;
            ws.Cells[row, 2].Value = submission.CompanyName;
            ws.Cells[row, 3].Value = submission.SubmittedAt;
            ws.Cells[row, 3].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";

            var responseMap = submission.Responses.ToDictionary(
                r => ExportFormatHelper.ColumnKey(r.Module, r.QuestionId),
                r => ExportFormatHelper.FormatAnswer(r.Answer));

            for (var c = 0; c < questionColumns.Count; c++)
            {
                if (responseMap.TryGetValue(questionColumns[c], out var answer))
                    ws.Cells[row, c + 4].Value = answer;
            }
        }

        ws.Cells[ws.Dimension!.Address].AutoFitColumns();
        ws.View.FreezePanes(2, 4);
    }

    private static void BuildLongSheet(ExcelPackage package, List<ZunoksSubmission> submissions)
    {
        var ws = package.Workbook.Worksheets.Add("Long Data");
        ws.Cells[1, 1].Value = "SubmissionId";
        ws.Cells[1, 2].Value = "CompanyName";
        ws.Cells[1, 3].Value = "SubmittedAt";
        ws.Cells[1, 4].Value = "Module";
        ws.Cells[1, 5].Value = "QuestionId";
        ws.Cells[1, 6].Value = "QuestionLabel";
        ws.Cells[1, 7].Value = "Answer";
        StyleHeader(ws.Cells[1, 1, 1, 7]);

        var row = 2;
        foreach (var submission in submissions)
        {
            foreach (var response in submission.Responses.OrderBy(r => r.Module).ThenBy(r => r.QuestionId))
            {
                ws.Cells[row, 1].Value = submission.Id;
                ws.Cells[row, 2].Value = submission.CompanyName;
                ws.Cells[row, 3].Value = submission.SubmittedAt;
                ws.Cells[row, 3].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
                ws.Cells[row, 4].Value = response.Module;
                ws.Cells[row, 5].Value = response.QuestionId;
                ws.Cells[row, 6].Value = response.QuestionLabel ?? response.QuestionId;
                ws.Cells[row, 7].Value = ExportFormatHelper.FormatAnswer(response.Answer);
                row++;
            }
        }

        ws.Cells[ws.Dimension!.Address].AutoFitColumns();
    }

    private static void BuildCodebookSheet(ExcelPackage package, List<ZunoksSubmission> submissions)
    {
        var ws = package.Workbook.Worksheets.Add("Codebook");
        ws.Cells[1, 1].Value = "ColumnKey";
        ws.Cells[1, 2].Value = "Module";
        ws.Cells[1, 3].Value = "QuestionId";
        ws.Cells[1, 4].Value = "QuestionLabel";
        StyleHeader(ws.Cells[1, 1, 1, 4]);

        var entries = submissions
            .SelectMany(s => s.Responses)
            .GroupBy(r => ExportFormatHelper.ColumnKey(r.Module, r.QuestionId))
            .Select(g => g.First())
            .OrderBy(r => r.Module)
            .ThenBy(r => r.QuestionId)
            .ToList();

        for (var i = 0; i < entries.Count; i++)
        {
            var row = i + 2;
            var entry = entries[i];
            ws.Cells[row, 1].Value = ExportFormatHelper.ColumnKey(entry.Module, entry.QuestionId);
            ws.Cells[row, 2].Value = entry.Module;
            ws.Cells[row, 3].Value = entry.QuestionId;
            ws.Cells[row, 4].Value = entry.QuestionLabel ?? entry.QuestionId;
        }

        ws.Cells[ws.Dimension!.Address].AutoFitColumns();
    }

    private static void StyleHeader(ExcelRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(238, 242, 255));
    }

    private static string SafeSheetName(string name)
    {
        var invalid = new[] { '\\', '/', '*', '?', ':', '[', ']' };
        var safe = new string(name.Select(ch => invalid.Contains(ch) ? ' ' : ch).ToArray()).Trim();
        return safe.Length > 31 ? safe[..31] : (string.IsNullOrWhiteSpace(safe) ? "Comparison" : safe);
    }
}
