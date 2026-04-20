using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public static class JsonParseErrorExtractor
{
    public static ParseError Extract(Exception ex)
    {
        if (ex is JsonException je)
        {
            var line = (int)((je.LineNumber ?? 0) + 1);
            var col = (int)((je.BytePositionInLine ?? 0) + 1);
            return new ParseError(line, col, je.Message, je.Path);
        }
        if (ex is FileNotFoundException fnf)
        {
            return new ParseError(0, 0, $"data.json not found at {fnf.FileName}", null);
        }
        return new ParseError(0, 0, ex.Message, null);
    }
}