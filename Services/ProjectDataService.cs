using System.Text.Json;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Services
{
    public class ProjectDataService
    {
        private ProjectData _cachedData;
        private DateTime _lastLoadTime = DateTime.MinValue;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProjectDataService> _logger;

        public ProjectDataService(IWebHostEnvironment environment, ILogger<ProjectDataService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<ProjectData> LoadProjectDataAsync(string jsonFileName = "data/data.json")
        {
            try
            {
                string wwwrootPath = _environment.WebRootPath;
                string jsonFilePath = Path.Combine(wwwrootPath, jsonFileName);

                if (!File.Exists(jsonFilePath))
                {
                    throw new DataLoadException(
                        $"data.json not found. Expected location: {jsonFilePath}. Please create the file with valid project data.");
                }

                string json = await File.ReadAllTextAsync(jsonFilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new DataLoadException("data.json is empty. Please add valid project data.");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                ProjectData data = JsonSerializer.Deserialize<ProjectData>(json, options);

                if (data == null)
                {
                    throw new DataLoadException(
                        "JSON deserialization resulted in null. Please verify data.json contains valid project structure.");
                }

                ValidateJsonSchema(data);

                _cachedData = data;
                _lastLoadTime = DateTime.Now;

                _logger.LogInformation("Project data loaded successfully at {LoadTime}", _lastLoadTime);

                return data;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error in data.json");
                throw new DataLoadException(
                    $"Invalid JSON format in data.json: {jsonEx.Message}. Please check the file syntax.",
                    jsonEx);
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "File I/O error reading data.json");
                throw new DataLoadException(
                    $"Error reading data.json: {ioEx.Message}. Please ensure the file is not locked by another process.",
                    ioEx);
            }
            catch (DataLoadException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading project data");
                throw new DataLoadException(
                    "An unexpected error occurred while loading project data. Please check the logs for details.",
                    ex);
            }
        }

        public ProjectData GetCachedData()
        {
            if (_cachedData == null)
            {
                throw new DataLoadException("No cached data available. Please load data first.");
            }

            return _cachedData;
        }

        public DateTime GetLastLoadTime()
        {
            return _lastLoadTime;
        }

        public void ValidateJsonSchema(ProjectData data)
        {
            var validationErrors = new List<string>();

            if (data.Project == null)
            {
                validationErrors.Add("Project section is missing.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.Project.Name))
                    validationErrors.Add("Project name is required.");
                if (data.Project.StartDate == default)
                    validationErrors.Add("Project startDate is required.");
                if (data.Project.EndDate == default)
                    validationErrors.Add("Project endDate is required.");
                if (data.Project.StartDate >= data.Project.EndDate)
                    validationErrors.Add("Project startDate must be before endDate.");
            }

            if (data.Milestones == null)
            {
                validationErrors.Add("Milestones section is missing.");
            }
            else
            {
                foreach (var milestone in data.Milestones)
                {
                    if (string.IsNullOrWhiteSpace(milestone.Id))
                        validationErrors.Add("Milestone Id is required.");
                    if (string.IsNullOrWhiteSpace(milestone.Name))
                        validationErrors.Add("Milestone name is required.");
                    if (milestone.TargetDate == default)
                        validationErrors.Add($"Milestone '{milestone.Name}' targetDate is required.");
                    if (milestone.CompletionPercentage < 0 || milestone.CompletionPercentage > 100)
                        validationErrors.Add($"Milestone '{milestone.Name}' completionPercentage must be 0-100.");
                }
            }

            if (data.Tasks == null)
            {
                validationErrors.Add("Tasks section is missing.");
            }
            else
            {
                foreach (var task in data.Tasks)
                {
                    if (string.IsNullOrWhiteSpace(task.Id))
                        validationErrors.Add("Task Id is required.");
                    if (string.IsNullOrWhiteSpace(task.Name))
                        validationErrors.Add("Task name is required.");
                    if (task.DueDate == default)
                        validationErrors.Add($"Task '{task.Name}' dueDate is required.");
                }
            }

            if (data.Metrics == null)
            {
                validationErrors.Add("Metrics section is missing.");
            }
            else
            {
                if (data.Metrics.TotalTasks < 0)
                    validationErrors.Add("Metrics totalTasks cannot be negative.");
                if (data.Metrics.CompletedTasks < 0)
                    validationErrors.Add("Metrics completedTasks cannot be negative.");
                if (data.Metrics.InProgressTasks < 0)
                    validationErrors.Add("Metrics inProgressTasks cannot be negative.");
                if (data.Metrics.CarriedOverTasks < 0)
                    validationErrors.Add("Metrics carriedOverTasks cannot be negative.");
                if (data.Metrics.CompletedTasks > data.Metrics.TotalTasks)
                    validationErrors.Add("Metrics completedTasks cannot exceed totalTasks.");
            }

            if (validationErrors.Count > 0)
            {
                string errorMessage = "Data validation failed:\n" + string.Join("\n", validationErrors);
                _logger.LogError(errorMessage);
                throw new DataLoadException(errorMessage);
            }
        }
    }
}