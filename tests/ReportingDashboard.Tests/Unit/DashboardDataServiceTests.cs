using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class DashboardDataServiceTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;

    public DashboardDataServiceTests()
    {
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns("/fake/wwwroot");
        _mockEnv.Setup(e => e.ContentRootPath).Returns("/fake");
        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    private DashboardDataService CreateService() =>
        new DashboardDataService(_mockEnv.Object, _mockLogger.Object);

    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_AcceptsRequiredDependencies_WithoutThrowing()
    {
        // Act
        var service = CreateService();

        // Assert
        service.Should().NotBeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_WithNullEnvironment_DoesNotThrow()
    {
        // The constructor does not validate null arguments.
        // Verify it accepts null without throwing.
        var service = new DashboardDataService(null!, _mockLogger.Object);
        service.Should().NotBeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        // The constructor does not validate null arguments.
        // Verify it accepts null without throwing.
        var service = new DashboardDataService(_mockEnv.Object, null!);
        service.Should().NotBeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Service_IsInstantiable_WithMockedDependencies()
    {
        // Arrange & Act
        var service = CreateService();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<DashboardDataService>();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Service_CanBeCreatedMultipleTimes_Independently()
    {
        // Act
        var service1 = CreateService();
        var service2 = CreateService();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().NotBeSameAs(service2);
    }
}