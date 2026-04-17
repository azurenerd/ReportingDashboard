var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "ReportingDashboard placeholder");
app.Run();