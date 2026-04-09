using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Interfaces
{
    public interface IDataValidator
    {
        /// <summary>
        /// Validate ProjectReport against schema rules.
        /// </summary>
        /// <param name="report">ProjectReport to validate (can be null)</param>
        /// <returns>ValidationResult with IsValid flag and errors collection</returns>
        /// <remarks>Does NOT throw exceptions; returns result object with errors.</remarks>
        ValidationResult Validate(ProjectReport report);
    }
}