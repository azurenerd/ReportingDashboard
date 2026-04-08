using System;
using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly ILogger<DataProvider> _logger;
    private readonly string _dataFilePath;
    private readonly IDataCache _dataCache;
    private const string CacheKey = "project_data";
    private const int CacheTtlMinutes = 60;

    private bool _isLoaded;
    private Project _projectData;

    public bool IsLoaded => _isLoaded;

    public DataProvider(ILogger<DataProvider> logger, IDataCache dataCache, string dataFilePath)
    {
        _logger = logger;
        _dataCache = dataCache;
        _dataFilePath = dataFilePath;
        _isLoaded = false;
        _projectData = null;
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        try
        {
            if (_isLoaded && _projectData != null)
            {
                _logger.LogInformation("Project data already loaded, returning cached instance");
                return _projectData;
            }

            if (!File.Exists(_dataFilePath))
            {
                var fileNotFoundMessage = $"data.json file not found at {_dataFilePath}";
                _logger.LogError(fileNotFoundMessage);
                throw new FileNotFoundException(fileNotFoundMessage, _dataFilePath);
            }

            var json = await File.ReadAllTextAsync(_dataFilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Project project;
            try
            {
                project = JsonSerializer.Deserialize<Project>(json, options);
            }
            catch (JsonException ex)
            {
                var jsonErrorMessage = $"Failed to deserialize data.json: {ex.Message}";
                _logger.LogError(ex, jsonErrorMessage);
                throw new JsonException(jsonErrorMessage, ex);
            }

            if (project == null)
            {
                var nullMessage = "data.json deserialized to null";
                _logger.LogError(nullMessage);
                throw new InvalidOperationException(nullMessage);
            }

            _projectData = _dataCache.GetOrCreate(
                CacheKey,
                () => project,
                TimeSpan.FromMinutes(CacheTtlMinutes)
            );

            _isLoaded = true;
            _logger.LogInformation("Project data loaded successfully from {DataFilePath}", _dataFilePath);
            return _projectData;
        }
        catch (Exception ex) when (!(ex is FileNotFoundException) && !(ex is JsonException) && !(ex is InvalidOperationException))
        {
            var unexpectedErrorMessage = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError(ex, unexpectedErrorMessage);
            throw new InvalidOperationException(unexpectedErrorMessage, ex);
        }
    }

    public Project GetProjectData()
    {
        if (!_isLoaded || _projectData == null)
        {
            var notLoadedMessage = "Project data has not been loaded. Call LoadProjectDataAsync() first.";
            _logger.LogError(notLoadedMessage);
            throw new InvalidOperationException(notLoadedMessage);
        }

        return _projectData;
    }

    public void InvalidateCache()
    {
        _dataCache.Remove(CacheKey);
        _projectData = null;
        _isLoaded = false;
        _logger.LogInformation("Project data cache invalidated");
    }
}