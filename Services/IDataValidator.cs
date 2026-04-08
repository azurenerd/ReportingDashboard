using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDataValidator
{
    ValidationResult ValidateProjectData(Project? project);
}