namespace AgentSquad.Runner.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();

    public ValidationResult() { }

    public ValidationResult(bool isValid, List<ValidationError>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? new List<ValidationError>();
    }

    public void AddError(ValidationError error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public void AddError(string errorCode, string message, string? fieldName = null)
    {
        Errors.Add(new ValidationError(errorCode, message, fieldName));
        IsValid = false;
    }

    public string GetErrorSummary()
    {
        if (IsValid) return "Validation passed";
        
        var errorFields = Errors
            .Where(e => e.FieldName != null)
            .Select(e => e.FieldName)
            .Distinct();
        
        return errorFields.Any()
            ? $"Validation failed for fields: {string.Join(", ", errorFields)}"
            : "Validation failed";
    }
}