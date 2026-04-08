using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IErrorHandler
{
    Task CaptureErrorAsync(Exception exception, ErrorContext context);
    string GetUserMessage(ErrorContext context, Exception exception);
}