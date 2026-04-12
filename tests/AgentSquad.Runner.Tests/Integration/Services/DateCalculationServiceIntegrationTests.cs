using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DateCalculationServiceIntegrationTests
{
    private readonly DateCalculationService _service;

    public DateCalculationServiceIntegrationTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DateCalculationService>();
        _service = new DateCalculationService(logger);
    }

    [Fact]
    public void GetDisplayMonths_IntegrationWithQuarters()
    {
        var currentDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc);
        
        var displayMonths = _service.GetDisplayMonths(currentDate);
        
        displayMonths.Should().NotBeEmpty();
        displayMonths.Should().HaveCountGreaterThanOrEqualTo(4);
        
        var currentMonth = displayMonths.FirstOrDefault(m => m.IsCurrentMonth);
        currentMonth.Should().NotBeNull();
        currentMonth!.Name.Should().Be("April");
        currentMonth.Year.Should().Be(2026);
    }

    [Fact]
    public void GetDisplayMonths_ProducesConsistentMonthSequence()
    {
        var currentDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc);
        
        var months1 = _service.GetDisplayMonths(currentDate);
        var months2 = _service.GetDisplayMonths(currentDate);
        
        months1.Should().HaveCount(months2.Count);
        for (int i = 0; i < months1.Count; i++)
        {
            months1[i].Name.Should().Be(months2[i].Name);
            months1[i].Year.Should().Be(months2[i].Year);
        }
    }

    [Fact]
    public void MilestoneCalculations_ConsistentAcrossMultipleCalls()
    {
        var baselineDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var milestoneDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        
        var position1 = _service.GetMilestoneXPosition(milestoneDate, baselineDate);
        var position2 = _service.GetMilestoneXPosition(milestoneDate, baselineDate);
        
        position1.Should().Be(position2);
    }

    [Fact]
    public void GetMilestoneXPosition_MarchCalculation()
    {
        var baselineDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var marchMilestone = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var position = _service.GetMilestoneXPosition(marchMilestone, baselineDate);
        
        position.Should().BeGreaterThan(0);
        position.Should().BeLessThan(1560);
    }

    [Fact]
    public void GetMilestoneXPosition_AprilCalculation()
    {
        var baselineDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var aprilMilestone = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var position = _service.GetMilestoneXPosition(aprilMilestone, baselineDate);
        
        position.Should().BeGreaterThan(0);
        position.Should().BeLessThan(1560);
    }

    [Fact]
    public void GetNowMarkerXPosition_LateMarch()
    {
        var baselineDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentDate = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc);
        
        var position = _service.GetNowMarkerXPosition(currentDate, baselineDate);
        
        position.Should().BeGreaterThan(0);
        position.Should().BeLessThan(1560);
    }

    [Fact]
    public void GetNowMarkerXPosition_EarlyApril()
    {
        var baselineDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentDate = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc);
        
        var position = _service.GetNowMarkerXPosition(currentDate, baselineDate);
        
        position.Should().BeGreaterThan(0);
        position.Should().BeLessThan(1560);
    }

    [Fact]
    public void MonthBoundsIncrementProperly()
    {
        var bounds0 = _service.GetMonthBounds(0);
        var bounds1 = _service.GetMonthBounds(1);
        var bounds2 = _service.GetMonthBounds(2);
        
        bounds0.startX.Should().BeLessThan(bounds1.startX);
        bounds1.startX.Should().BeLessThan(bounds2.startX);
        bounds0.endX.Should().Equal(bounds1.startX);
        bounds1.endX.Should().Equal(bounds2.startX);
    }

    [Fact]
    public void IsCurrentMonth_MatchesDisplayMonths()
    {
        var currentDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc);
        var displayMonths = _service.GetDisplayMonths(currentDate);
        
        var currentMonthResult = displayMonths.FirstOrDefault(m => m.IsCurrentMonth);
        currentMonthResult.Should().NotBeNull();
        
        var isCurrentFromService = _service.IsCurrentMonth(currentMonthResult!.Name, currentMonthResult.Year, currentDate);
        isCurrentFromService.Should().BeTrue();
    }

    [Fact]
    public void GetDisplayMonths_CrossYearBoundary()
    {
        var currentDate = new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc);
        
        var displayMonths = _service.GetDisplayMonths(currentDate);
        
        displayMonths.Should().NotBeEmpty();
        var currentMonth = displayMonths.FirstOrDefault(m => m.IsCurrentMonth);
        currentMonth.Should().NotBeNull();
    }
}