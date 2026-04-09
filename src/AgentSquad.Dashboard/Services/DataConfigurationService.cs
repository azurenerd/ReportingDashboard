using System.Text.Json;
using AgentSquad.Dashboard.Models;
using FluentValidation;
using FluentValidation.Results;

namespace AgentSquad.Dashboard.Services;

public class DataConfigurationService
{
    private readonly ILogger<DataConfigurationService> _logger;
    private readonly IValidator<ProjectData> _validator;

    public DataConfigurationService(ILogger<DataConfigurationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = new ProjectDataValidator();
    }

    public async Task<ProjectData> LoadConfigurationAsync(string filePath)
    {
        _logger.LogInformation("Loading data.json from {FilePath}", filePath);

        if (!FileExists(filePath))
        {
            throw new FileNotFoundException(
                $"data.json not found at {filePath}. Ensure file exists in application wwwroot folder.");
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<ProjectData>(json) ?? new ProjectData();

            var validationResult = ValidateProjectData(data);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ConfigurationException(
                    "Validation failed. Errors: " + errors,
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            _logger.LogInformation("Loaded and validated project: {ProjectName}", data.Project.Name);
            return data;
        }
        catch (JsonException ex)
        {
            var message = $"Invalid JSON format: {ex.Message}";
            _logger.LogError(message);
            throw new JsonException(message, ex);
        }
    }

    public ValidationResult ValidateProjectData(ProjectData data)
    {
        return _validator.Validate(data);
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }
}

public class ProjectDataValidator : AbstractValidator<ProjectData>
{
    public ProjectDataValidator()
    {
        RuleFor(x => x.Project)
            .NotNull()
            .SetValidator(new ProjectInfoValidator());

        RuleFor(x => x.Milestones)
            .NotNull()
            .Must(m => m.Count >= 5 && m.Count <= 10)
            .WithMessage("Must have 5-10 milestones");

        RuleFor(x => x.Progress)
            .NotNull()
            .SetValidator(new ProgressDataValidator());

        RuleFor(x => x.Metrics)
            .NotNull()
            .SetValidator(new MetricsDataValidator());
    }
}

public class ProjectInfoValidator : AbstractValidator<ProjectInfo>
{
    public ProjectInfoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Owner).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status)
            .Must(s => new[] { "On Track", "At Risk", "Blocked" }.Contains(s))
            .WithMessage("Status must be 'On Track', 'At Risk', or 'Blocked'");
    }
}

public class ProgressDataValidator : AbstractValidator<ProgressData>
{
    public ProgressDataValidator()
    {
        RuleFor(x => x.Shipped).NotNull();
        RuleFor(x => x.InProgress).NotNull();
        RuleFor(x => x.CarriedOver).NotNull();

        RuleFor(x => x.Shipped.Count + x.InProgress.Count + x.CarriedOver.Count)
            .LessThanOrEqualTo(50)
            .WithMessage("Total work items must be 50 or fewer");
    }
}

public class MetricsDataValidator : AbstractValidator<MetricsData>
{
    public MetricsDataValidator()
    {
        RuleFor(x => x.TotalItems)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1000);

        RuleFor(x => x.ShippedCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1000);

        RuleFor(x => x.InProgressCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1000);

        RuleFor(x => x.CarriedOverCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1000);
    }
}

public class ConfigurationException : Exception
{
    public List<string> ValidationErrors { get; set; }

    public ConfigurationException(string message, List<string>? errors = null)
        : base(message)
    {
        ValidationErrors = errors ?? new();
    }
}