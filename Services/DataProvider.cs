using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly IDataValidator _validator;
    private readonly ILogger<DataProvider> _logger;
    private const string DATA_FILE_PATH = "wwwroot/data.json";
    private const string CACHE_KEY = "project_data";

    public DataProvider(IDataCache cache, IDataValidator validator, ILogger<DataProvider> logger)
    {
        _cache = cache;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        _logger.LogInformation("[{Timestamp}] Loading project data from {FilePath}", 
            DateTime.UtcNow:O, DATA_FILE_PATH);

        try
        {
            var cached = await _cache.GetAsync<Project>(CACHE_KEY);
            if (cached != null)
            {
                _logger.LogInformation("[{Timestamp}] Project data loaded from cache", 
                    DateTime.UtcNow:O);
                return cached;
            }

            _logger.LogInformation("[{Timestamp}] Cache miss, reading from file", 
                DateTime.UtcNow:O);

            string json;
            try
            {
                json = await File.ReadAllTextAsync(DATA_FILE_PATH);
                _logger.LogInformation("[{Timestamp}] Successfully read data.json ({SizeBytes} bytes)", 
                    DateTime.UtcNow:O, json.Length);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "[{Timestamp}] FileNotFoundException: File not found at {FilePath}", 
                    DateTime.UtcNow:O, DATA_FILE_PATH);
                throw new FileNotFoundException(
                    "Configuration file missing. Please ensure data.json exists in the application directory.",
                    DATA_FILE_PATH,
                    ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "[{Timestamp}] UnauthorizedAccessException: Access denied to {FilePath}", 
                    DateTime.UtcNow:O, DATA_FILE_PATH);
                throw new InvalidOperationException(
                    "Access denied reading configuration file. Please check file permissions.",
                    ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "[{Timestamp}] IOException: Error reading {FilePath}", 
                    DateTime.UtcNow:O, DATA_FILE_PATH);
                throw new InvalidOperationException(
                    $"Error reading configuration file: {ex.Message}",
                    ex);
            }

            Project? project;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };
                project = JsonSerializer.Deserialize<Project>(json, options);
                _logger.LogInformation("[{Timestamp}] Successfully parsed JSON", DateTime.UtcNow:O);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, 
                    "[{Timestamp}] JsonException: Invalid JSON at line {LineNumber}, column {Column}\nException: {Message}", 
                    DateTime.UtcNow:O, ex.LineNumber, ex.BytePositionInLine, ex.Message);
                throw new JsonException(
                    "Invalid JSON format in configuration file. Please check data.json syntax.",
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Timestamp}] Unexpected error during JSON parsing: {ExceptionType}: {Message}", 
                    DateTime.UtcNow:O, ex.GetType().Name, ex.Message);
                throw new InvalidOperationException(
                    "Unexpected error parsing configuration file.",
                    ex);
            }

            var validationResult = _validator.ValidateProjectData(project);
            if (!validationResult.IsValid)
            {
                var missingFields = validationResult.Errors
                    .Where(e => !string.IsNullOrEmpty(e.FieldName))
                    .Select(e => e.FieldName)
                    .Distinct()
                    .ToList();

                var errorMessage = missingFields.Any()
                    ? $"Configuration validation failed. Required fields missing: {string.Join(", ", missingFields)}"
                    : "Configuration validation failed. Please verify data.json structure.";

                _logger.LogError(
                    "[{Timestamp}] InvalidOperationException: Validation failed with {ErrorCount} errors:\n{Errors}",
                    DateTime.UtcNow:O,
                    validationResult.Errors.Count,
                    string.Join("\n", validationResult.Errors.Select(e => $"  - {e.ToString()}")));

                throw new InvalidOperationException(errorMessage);
            }

            await _cache.SetAsync(CACHE_KEY, project, TimeSpan.FromHours(1));
            _logger.LogInformation("[{Timestamp}] Project data cached successfully (1 hour TTL)", 
                DateTime.UtcNow:O);

            return project;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "[{Timestamp}] Unexpected exception type {ExceptionType}: {Message}\n{StackTrace}", 
                DateTime.UtcNow:O, ex.GetType().Name, ex.Message, ex.StackTrace);
            throw new InvalidOperationException(
                "An unexpected error occurred while loading project data.",
                ex);
        }
    }

    public void InvalidateCache()
    {
        _cache.Remove(CACHE_KEY);
        _logger.LogInformation("[{Timestamp}] Project data cache invalidated", DateTime.UtcNow:O);
    }
}