using Bunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Base class for bUnit component tests providing a shared TestContext.
/// </summary>
public abstract class BunitTestBase : IDisposable
{
    protected Bunit.TestContext Context { get; }

    protected BunitTestBase()
    {
        Context = new Bunit.TestContext();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}