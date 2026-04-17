namespace ReportingDashboard.Services;

using System.Text.Json;
using ReportingDashboard.Models;

public class DashboardDataService
{
    private readonly string _dataDirectory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataDirectory = Path.Combine(env.WebRootPath, "data");
    }

    public async Task<DashboardData> LoadAsync(string? filename = null)
    {
        var targetFile = SanitizeFilename(filename ?? "data.json");
        var filePath = Path.Combine(_dataDirectory, targetFile);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"{targetFile} not found. Place your data file at wwwroot/data/{targetFile}.");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");

        if (data.SchemaVersion != 1)
        {
            throw new InvalidOperationException(
                $"Unsupported schema version: {data.SchemaVersion}. Expected: 1.");
        }

        return data;
    }

    private static string SanitizeFilename(string filename)
    {
        if (filename.Contains("..") || filename.Contains('/') || filename.Contains('\\'))
        {
            throw new ArgumentException(
                $"Invalid filename: '{filename}'. Path separators and '..' are not allowed.");
        }

        if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid filename: '{filename}'. Only .json files are supported.");
        }

        return filename;
    }
}