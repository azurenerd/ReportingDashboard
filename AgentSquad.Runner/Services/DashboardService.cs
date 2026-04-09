using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services.Exceptions;

namespace AgentSquad.Runner.Services;

public class DashboardService : IDashboardService
{
    private readonly ILogger<DashboardService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashboardService(ILogger<DashboardService> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<DashboardData> LoadDataAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                var errorMessage = $"Dashboard data file not found at {filePath}";
                _logger.LogError(errorMessage);
                throw new DashboardLoadException(errorMessage);
            }

            var json = await File.ReadAllTextAsync(filePath);

            DashboardData? data;
            try
            {
                data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
            }
            catch (JsonException ex)
            {
                var errorMessage = $"Invalid JSON format in data.json at line {ex.LineNumber}, position {ex.BytePositionInLine}";
                _logger.LogError(ex, errorMessage);
                throw new DashboardLoadException(errorMessage, ex);
            }

            if (data == null)
            {
                var errorMessage = "Dashboard data deserialization resulted in null object";
                _logger.LogError(errorMessage);
                throw new DashboardLoadException(errorMessage);
            }

            var isValid = await ValidateDataAsync(data);
            if (!isValid)
            {
                var errorMessage = "Dashboard data validation failed. Check required fields and data types.";
                _logger.LogError(errorMessage);
                throw new DashboardLoadException(errorMessage);
            }

            _logger.LogInformation("Dashboard data loaded successfully from {FilePath}", filePath);
            return data;
        }
        catch (DashboardLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unable to read dashboard data from {filePath}";
            _logger.LogError(ex, errorMessage);
            throw new DashboardLoadException(errorMessage, ex);
        }
    }

    public async Task<bool> ValidateDataAsync(DashboardData data)
    {
        if (data == null)
        {
            _logger.LogError("DashboardData object is null");
            return false;
        }

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(data, null, null);

        var isValid = Validator.TryValidateObject(data, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            foreach (var result in validationResults)
            {
                _logger.LogError("Validation error: {ErrorMessage}", result.ErrorMessage);
            }
            return false;
        }

        // Custom validation rules
        if (data.Milestones != null)
        {
            var milestoneLookup = data.Milestones.ToDictionary(m => m.Id);

            // Verify milestone IDs are unique
            if (milestoneLookup.Count != data.Milestones.Count)
            {
                _logger.LogError("Duplicate milestone IDs detected");
                return false;
            }

            // Verify milestones are sorted by date
            var sortedMilestones = data.Milestones.OrderBy(m => m.Date).ToList();
            for (int i = 0; i < data.Milestones.Count; i++)
            {
                if (!data.Milestones[i].Id.Equals(sortedMilestones[i].Id))
                {
                    _logger.LogWarning("Milestones are not sorted by date. Consider sorting for consistency.");
                    break;
                }
            }
        }

        if (data.WorkItems != null && data.Milestones != null)
        {
            var milestoneLookup = data.Milestones.ToDictionary(m => m.Id);
            var workItemLookup = new Dictionary<string, int>();

            // Verify work item IDs are unique
            foreach (var item in data.WorkItems)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    _logger.LogError("Work item with empty ID detected");
                    return false;
                }

                if (workItemLookup.TryGetValue(item.Id, out _))
                {
                    _logger.LogError("Duplicate work item ID detected: {Id}", item.Id);
                    return false;
                }
                workItemLookup[item.Id] = 1;

                // Verify MilestoneId references existing milestone
                if (!string.IsNullOrEmpty(item.MilestoneId) && !milestoneLookup.ContainsKey(item.MilestoneId))
                {
                    _logger.LogError("Work item {Id} references non-existent milestone {MilestoneId}", item.Id, item.MilestoneId);
                    return false;
                }
            }
        }

        return true;
    }

    public async IAsyncEnumerable<DashboardData> WatchDataAsync(string filePath)
    {
        using var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath) ?? ".")
        {
            Filter = Path.GetFileName(filePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        var taskCompletionSource = new TaskCompletionSource<bool>();

        void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                taskCompletionSource.TrySetResult(true);
            }
        }

        watcher.Changed += OnChanged;
        watcher.EnableRaisingEvents = true;

        try
        {
            while (true)
            {
                await taskCompletionSource.Task;
                taskCompletionSource = new TaskCompletionSource<bool>();

                // Add small delay to ensure file is finished writing
                await Task.Delay(100);

                try
                {
                    var data = await LoadDataAsync(filePath);
                    _logger.LogInformation("Dashboard data reloaded from file watch");
                    yield return data;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading dashboard data during file watch");
                }
            }
        }
        finally
        {
            watcher.Changed -= OnChanged;
            watcher.Dispose();
        }
    }
}