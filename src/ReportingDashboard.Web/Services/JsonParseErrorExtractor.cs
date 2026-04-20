using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// Stub — downstream task T4 will enrich mapping for FileNotFound / IO / schema cases.
public static class JsonParseErrorExtractor
{
    public static ParseError Extract(Exception ex)
    {
        if (ex is JsonException jex)
        {
            return new ParseError(
                Line: (int)(jex.LineNumber ?? 0) + 1,
                Column: (int)(jex.BytePositionInLine ?? 0) + 1,
                Message: jex.Message,
                JsonPath: jex.Path);
        }

        return new ParseError(0, 0, ex.Message, null);
    }
}