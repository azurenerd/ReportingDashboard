using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public static class JsonParseErrorExtractor
{
    public static ParseError Extract(Exception ex) =>
        new(Line: 0, Column: 0, Message: ex.Message, JsonPath: null);
}