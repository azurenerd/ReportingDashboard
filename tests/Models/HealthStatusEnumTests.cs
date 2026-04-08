using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Models;

public class HealthStatusEnumTests
{
    [Fact]
    public void HealthStatus_OnTrack_HasValue0()
    {
        Assert.Equal(0, (int)HealthStatus.OnTrack);
    }

    [Fact]
    public void HealthStatus_AtRisk_HasValue1()
    {
        Assert.Equal(1, (int)HealthStatus.AtRisk);
    }

    [Fact]
    public void HealthStatus_Blocked_HasValue2()
    {
        Assert.Equal(2, (int)HealthStatus.Blocked);
    }

    [Fact]
    public void HealthStatus_GetNames_ReturnsAllThreeValues()
    {
        var names = Enum.GetNames(typeof(HealthStatus));

        Assert.Equal(3, names.Length);
        Assert.Contains("OnTrack", names);
        Assert.Contains("AtRisk", names);
        Assert.Contains("Blocked", names);
    }

    [Fact]
    public void HealthStatus_GetValues_ReturnsAllThreeValues()
    {
        var values = Enum.GetValues(typeof(HealthStatus)).Cast<HealthStatus>();

        Assert.Contains(HealthStatus.OnTrack, values);
        Assert.Contains(HealthStatus.AtRisk, values);
        Assert.Contains(HealthStatus.Blocked, values);
    }

    [Fact]
    public void HealthStatus_Cast_FromInt()
    {
        var onTrack = (HealthStatus)0;
        var atRisk = (HealthStatus)1;
        var blocked = (HealthStatus)2;

        Assert.Equal(HealthStatus.OnTrack, onTrack);
        Assert.Equal(HealthStatus.AtRisk, atRisk);
        Assert.Equal(HealthStatus.Blocked, blocked);
    }

    [Fact]
    public void HealthStatus_Parse_FromString()
    {
        var onTrack = Enum.Parse<HealthStatus>("OnTrack");
        var atRisk = Enum.Parse<HealthStatus>("AtRisk");
        var blocked = Enum.Parse<HealthStatus>("Blocked");

        Assert.Equal(HealthStatus.OnTrack, onTrack);
        Assert.Equal(HealthStatus.AtRisk, atRisk);
        Assert.Equal(HealthStatus.Blocked, blocked);
    }

    [Fact]
    public void HealthStatus_ToString_ReturnsCorrectString()
    {
        Assert.Equal("OnTrack", HealthStatus.OnTrack.ToString());
        Assert.Equal("AtRisk", HealthStatus.AtRisk.ToString());
        Assert.Equal("Blocked", HealthStatus.Blocked.ToString());
    }
}