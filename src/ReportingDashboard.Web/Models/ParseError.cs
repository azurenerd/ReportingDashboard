namespace ReportingDashboard.Web.Models;

public sealed record ParseError(
    int Line,
    int Column,
    string Message,
    string? JsonPath);