using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public interface IDataProvider
    {
        Task<Project> GetProjectDataAsync();
        bool IsLoaded { get; }
        string ErrorMessage { get; }
    }

    public class DataProvider : IDataProvider
    {
        private readonly IDataCache _cache;
        private readonly string _dataFilePath;
        private bool _isLoaded = false;
        private string _errorMessage = string.Empty;

        public bool IsLoaded => _isLoaded;
        public string ErrorMessage => _errorMessage;

        public DataProvider(IDataCache cache)
        {
            _cache = cache;
            _dataFilePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "data.json");
        }

        public async Task<Project> GetProjectDataAsync()
        {
            const string cacheKey = "project_data";

            try
            {
                var cached = await _cache.GetAsync<Project>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                if (!File.Exists(_dataFilePath))
                {
                    _errorMessage = $"Data file not found at {_dataFilePath}";
                    _isLoaded = false;
                    return null;
                }

                var json = await File.ReadAllTextAsync(_dataFilePath);
                var project = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (project == null)
                {
                    _errorMessage = "Failed to deserialize data.json";
                    _isLoaded = false;
                    return null;
                }

                await _cache.SetAsync(cacheKey, project);
                _isLoaded = true;
                _errorMessage = string.Empty;
                return project;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Error loading data.json: {ex.Message}";
                _isLoaded = false;
                return null;
            }
        }
    }
}