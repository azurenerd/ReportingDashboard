namespace AgentSquad.Runner.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Validators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Singleton service for loading, deserializing, validating, and caching dashboard data from JSON.
/// 
/// Lifecycle:
///   1. Registered as Singleton in Program.cs DI container
///   2. InitializeAsync() called before app.Run() in Program.cs
///   3. Cached ProjectDashboard available to all Blazor components via DI injection
///   4. RefreshAsync() reserved for Phase 2 (hot-reload via file watcher)
/// 
/// Error Handling Strategy:
///   - Exceptions during InitializeAsync() are NOT rethrown (graceful degradation)
///   - _lastError populated with user-friendly message
///   - Application continues; error displayed via DashboardPage error alert
///   - GetDashboard() throws InvalidOperationException if not initialized
/// </summary>
public class ProjectDataService : IProjectDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProjectDataService> _logger;

    /// <summary>
    /// JSON deserialization options:
    /// - PropertyNameCaseInsensitive: Maps JSON camelCase to C# PascalCase
    /// - PropertyNamingPolicy: Explicitly set to CamelCase for symmetry
    /// - WriteIndented: false (no pretty-printing needed for deserialization)
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private ProjectDashboard? _cache;
    private string? _lastError;

    public ProjectDataService(IWebHostEnvironment env, ILogger<ProjectDataService> logger)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsInitialized => _cache != null && _lastError == null;

    public string? LastError => _lastError;

    /// <summary>
    /// Initialize dashboard data from data.json at startup.
    /// 
    /// Steps:
    ///   1. Construct safe file path: ContentRootPath/data/data.json
    ///   2. Verify path is within ContentRootPath (path traversal defense)
    ///   3. Read and deserialize JSON to ProjectDashboard
    ///   4. Validate schema (ProjectDashboardValidator)
    ///   5. Calculate ProgressMetrics from work items
    ///   6. Cache result in _cache
    ///   7. Log success
    /// 
    /// On error:
    ///   - Populate _lastError with user-friendly message
    ///   - Log detailed error to console (includes stack trace)
    ///   - Do NOT rethrow (allows graceful degradation in UI)
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var dataPath = Path.Combine(_env.ContentRootPath, "data", "data.json");

            // Path traversal defense: verify resolved path within ContentRootPath
            var resolvedPath = Path.GetFullPath(dataPath);
            var resolvedRoot = Path.GetFullPath(_env.ContentRootPath);

            if (!resolvedPath.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException($"Path traversal attempt detected: {dataPath}");
            }

            if (!File.Exists(dataPath))
            {
                throw new FileNotFoundException($"Data file not found: {dataPath}");
            }

            _logger.LogInformation("Loading dashboard from {DataPath}", dataPath);

            // Deserialize JSON to ProjectDashboard
            using var fileStream = File.OpenRead(dataPath);
            var dashboard = await JsonSerializer.DeserializeAsync<ProjectDashboard>(
                fileStream,
                JsonOptions
            );

            if (dashboard == null)
            {
                throw new InvalidOperationException("Deserialized dashboard data is null");
            }

            // Validate schema
            ProjectDashboardValidator.Validate(dashboard);

            // Calculate metrics from work items
            CalculateMetrics(dashboard);

            // Cache result
            _cache = dashboard;
            _lastError = null;

            _logger.LogInformation(
                "Dashboard loaded successfully: {ProjectName} ({Milestones} milestones, {Items} items)",
                dashboard.ProjectName,
                dashboard.Milestones?.Count ?? 0,
                (dashboard.Shipped?.Count ?? 0)
                    + (dashboard.InProgress?.Count ?? 0)
                    + (dashboard.CarriedOver?.Count ?? 0)
            );
        }
        catch (FileNotFoundException ex)
        {
            _lastError = $"Data file not found: data/data.json. Create file with sample data.";
            _logger.LogError(ex, "Failed to load dashboard: file not found");
        }
        catch (JsonException ex)
        {
            _lastError = $"JSON syntax error: {ex.Message}. Check data.json for typos or formatting errors.";
            _logger.LogError(ex, "Failed to load dashboard: JSON error");
        }
        catch (InvalidOperationException ex)
        {
            _lastError = $"Data validation failed: {ex.Message}";
            _logger.LogError(ex, "Failed to load dashboard: validation error");
        }
        catch (Exception ex)
        {
            _lastError = $"Unexpected error loading dashboard: {ex.Message}";
            _logger.LogError(ex, "Failed to load dashboard: unexpected error");
        }
    }

    /// <summary>
    /// Get the cached ProjectDashboard.
    /// 
    /// Returns immutable reference to cached data loaded during InitializeAsync().
    /// Safe to call multiple times; returns same reference.
    /// </summary>
    /// <returns>Cached ProjectDashboard</returns>
    /// <exception cref="InvalidOperationException">If InitializeAsync() not called or failed</exception>
    public ProjectDashboard GetDashboard()
    {
        if (_cache == null)
        {
            throw new InvalidOperationException(
                "Dashboard data not initialized. Call InitializeAsync() before accessing dashboard."
            );
        }

        return _cache;
    }

    /// <summary>
    /// Reload data from disk (Phase 2 placeholder).
    /// 
    /// Future implementation will:
    ///   - Re-read data.json from disk
    ///   - Re-validate schema
    ///   - Re-calculate metrics
    ///   - Update _cache reference
    ///   - Notify Blazor components via SignalR that data changed (StateHasChanged)
    /// </summary>
    public async Task RefreshAsync()
    {
        // Stub for Phase 2: file watcher + SignalR hot-reload
        await Task.CompletedTask;
    }

    /// <summary>
    /// Calculate ProgressMetrics from work item counts.
    /// 
    /// Formulas:
    ///   TotalPlanned = Shipped + InProgress + CarriedOver
    ///   Completed = Shipped
    ///   InFlight = InProgress + CarriedOver
    ///   HealthScore = (Completed / TotalPlanned) * 100, range 0-100
    /// 
    /// Handles division-by-zero: HealthScore defaults to 0 if TotalPlanned is 0.
    /// </summary>
    private static void CalculateMetrics(ProjectDashboard dashboard)
    {
        var shipped = dashboard.Shipped?.Count ?? 0;
        var inProgress = dashboard.InProgress?.Count ?? 0;
        var carriedOver = dashboard.CarriedOver?.Count ?? 0;

        var totalPlanned = shipped + inProgress + carriedOver;
        var completed = shipped;
        var inFlight = inProgress + carriedOver;

        var healthScore = totalPlanned > 0
            ? (completed / (decimal)totalPlanned) * 100m
            : 0m;

        dashboard.Metrics = new ProgressMetrics
        {
            TotalPlanned = totalPlanned,
            Completed = completed,
            InFlight = inFlight,
            HealthScore = Math.Round(healthScore, 2),
        };
    }
}