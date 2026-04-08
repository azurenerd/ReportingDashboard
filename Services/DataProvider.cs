using System;
using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly ILogger<DataProvider> _logger;
    private readonly string _dataFilePath;
    private Project _cachedProjectData;
    private bool _isLoaded;
    private string _errorMessage;

    public bool IsLoaded => _isLoaded;
    public string ErrorMessage => _errorMessage;

    public DataProvider(ILogger<DataProvider> logger, string dataFilePath)
    {
        _logger = logger;
        _dataFilePath = dataFilePath;
        _isLoaded = false;
        _errorMessage = null;

        LoadData();
    }

    public Project GetProjectData()
    {
        if (!_isLoaded)
        {
            throw new InvalidOperationException($"Project data not loaded. Error: {_errorMessage}");
        }

        return _cachedProjectData;
    }

    private void LoadData()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                _errorMessage = $"data.json file not found at {_dataFilePath}";
                _logger.LogError(_errorMessage);
                return;
            }

            var json = File.ReadAllText(_dataFilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                _cachedProjectData = JsonSerializer.Deserialize<Project>(json, options);
            }
            catch (JsonException ex)
            {
                _errorMessage = $"Failed to deserialize data.json: {ex.Message}";
                _logger.LogError(_errorMessage);
                return;
            }

            if (_cachedProjectData == null)
            {
                _errorMessage = "data.json deserialized to null";
                _logger.LogError(_errorMessage);
                return;
            }

            _isLoaded = true;
            _logger.LogInformation("Project data loaded successfully from {DataFilePath}", _dataFilePath);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError(ex, _errorMessage);
        }
    }
}