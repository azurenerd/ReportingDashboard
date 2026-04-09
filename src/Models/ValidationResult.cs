using System.Collections.Generic;

namespace AgentSquad.Runner.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
    }

    public class ValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}