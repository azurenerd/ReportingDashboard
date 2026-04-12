using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardOptionsTests
{
    [Fact]
    public void DashboardOptions_HasDefaultDataFilePath()
    {
        var options = new DashboardOptions();
        options.DataFilePath.Should().Be("./wwwroot/data/data.json");
    }

    [Fact]
    public void DashboardOptions_HasDefaultAdoBacklogUrl()
    {
        var options = new DashboardOptions();
        options.AdoBacklogUrl.Should().Be("https://dev.azure.com/");
    }

    [Fact]
    public void DashboardOptions_HasDefaultPort()
    {
        var options = new DashboardOptions();
        options.Port.Should().Be(5000);
    }

    [Fact]
    public void DashboardOptions_HasDefaultAllowRemoteAccessFalse()
    {
        var options = new DashboardOptions();
        options.AllowRemoteAccess.Should().BeFalse();
    }

    [Fact]
    public void DashboardOptions_CanSetDataFilePath()
    {
        var options = new DashboardOptions { DataFilePath = "/custom/path/data.json" };
        options.DataFilePath.Should().Be("/custom/path/data.json");
    }

    [Fact]
    public void DashboardOptions_CanSetAdoBacklogUrl()
    {
        var options = new DashboardOptions { AdoBacklogUrl = "https://custom.azuredevops.com/" };
        options.AdoBacklogUrl.Should().Be("https://custom.azuredevops.com/");
    }

    [Fact]
    public void DashboardOptions_CanSetPort()
    {
        var options = new DashboardOptions { Port = 8080 };
        options.Port.Should().Be(8080);
    }

    [Fact]
    public void DashboardOptions_CanSetAllowRemoteAccess()
    {
        var options = new DashboardOptions { AllowRemoteAccess = true };
        options.AllowRemoteAccess.Should().BeTrue();
    }

    [Fact]
    public void DashboardOptions_CanBeInitializedWithAllProperties()
    {
        var options = new DashboardOptions
        {
            DataFilePath = "/data/custom.json",
            AdoBacklogUrl = "https://custom.ado.com/",
            Port = 9000,
            AllowRemoteAccess = true
        };

        options.DataFilePath.Should().Be("/data/custom.json");
        options.AdoBacklogUrl.Should().Be("https://custom.ado.com/");
        options.Port.Should().Be(9000);
        options.AllowRemoteAccess.Should().BeTrue();
    }

    [Fact]
    public void DashboardOptions_SupportsPortRange()
    {
        var validPorts = new[] { 80, 443, 5000, 8000, 8080, 3000 };

        foreach (var port in validPorts)
        {
            var options = new DashboardOptions { Port = port };
            options.Port.Should().Be(port);
        }
    }
}