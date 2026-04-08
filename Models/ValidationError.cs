namespace AgentSquad.Runner.Models;

public class ValidationError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ValidationError() { }

    public ValidationError(string errorCode, string message, string? fieldName = null)
    {
        ErrorCode = errorCode;
        Message = message;
        FieldName = fieldName;
        Timestamp = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return FieldName != null
            ? $"{ErrorCode}: {Message} (Field: {FieldName})"
            : $"{ErrorCode}: {Message}";
    }
}