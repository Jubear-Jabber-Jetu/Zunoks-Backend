using System.Text.Json;

namespace ZunoksBackend.Services;

public static class ExportFormatHelper
{
    public static string ColumnKey(string module, string questionId) => $"{module} | {questionId}";

    public static string FormatAnswer(string? answer)
    {
        if (string.IsNullOrWhiteSpace(answer)) return string.Empty;

        var trimmed = answer.Trim();
        if (!trimmed.StartsWith('{') && !trimmed.StartsWith('['))
            return NormalizeExportText(trimmed);

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            return NormalizeExportText(FormatJsonElement(doc.RootElement));
        }
        catch
        {
            return NormalizeExportText(trimmed);
        }
    }

    public static string NormalizeExportText(string value)
    {
        return value
            .Replace('\u2014', '-') // em dash
            .Replace('\u2013', '-'); // en dash
    }

    private static string FormatJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => string.Join(" | ", element.EnumerateObject()
                .Select(p => FormatObjectProperty(p.Name, p.Value))),
            JsonValueKind.Array => string.Join("; ", element.EnumerateArray().Select(e => e.ToString())),
            _ => element.ToString()
        };
    }

    private static string FormatObjectProperty(string name, JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            return string.Join("; ", value.EnumerateObject()
                .Select(p => $"{name}.{p.Name}: {p.Value}"));
        }

        return $"{name}: {value}";
    }
}
