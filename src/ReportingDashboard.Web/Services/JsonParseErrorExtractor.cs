using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public static class JsonParseErrorExtractor
{
    public static ParseError Extract(Exception ex)
    {
        // Stub: real mapping owned by T4.
        return new ParseError(
            Line: 0,
            Column: 0,
            Message: ex?.Message ?? "unknown parse error",
            JsonPath: null);
    }
}