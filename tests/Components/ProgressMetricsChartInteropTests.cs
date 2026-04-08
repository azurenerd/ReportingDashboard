using System;
using System.Collections.Generic;
using Bunit;
using Microsoft.JSInterop;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using Moq;

namespace AgentSquad.Runner.Tests.Components;

public class ProgressMetricsChartInteropTests : TestContext
{
    [Fact]
    public async Task ProgressMetrics_InitializesBurndownChartOnFirstRender()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()))
            .ReturnsAsync(true);

        Services.AddScoped(_ => jsRuntimeMock.Object);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20),
            EstimatedBurndownRate = 2.5
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Act - Wait for async operations
        await component.Instance.Task;

        // Assert
        jsRuntimeMock.Verify(
            x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.Is<object[]>(args => args.Length > 0 && args[0].ToString() == "burndownChart")),
            Times.Once);
    }

    [Fact]
    public void ProgressMetrics_WithNullMetrics_DoesNotInitializeChart()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        Services.AddScoped(_ => jsRuntimeMock.Object);

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, null)
        );

        // Assert
        jsRuntimeMock.Verify(
            x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public void ProgressMetrics_WithZeroTasks_DoesNotInitializeChart()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        Services.AddScoped(_ => jsRuntimeMock.Object);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 0,
            CompletedTasks = 0,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        jsRuntimeMock.Verify(
            x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task ProgressMetrics_ChartInitialization_IncludesCanvasElementId()
    {
        // Arrange
        var capturedArgs = new List<object[]>();
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) => capturedArgs.Add(args))
            .ReturnsAsync(true);

        Services.AddScoped(_ => jsRuntimeMock.Object);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        await component.Instance.Task;

        // Assert
        Assert.NotEmpty(capturedArgs);
        var args = capturedArgs[0];
        Assert.Equal("burndownChart", args[0].ToString());
    }

    [Fact]
    public async Task ProgressMetrics_ChartInitialization_PassesLabelsArray()
    {
        // Arrange
        object[] capturedArgs = null;
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) => capturedArgs = args)
            .ReturnsAsync(true);

        Services.AddScoped(_ => jsRuntimeMock.Object);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        await component.Instance.Task;

        // Assert
        Assert.NotNull(capturedArgs);
        Assert.True(capturedArgs.Length >= 2);
        Assert.NotNull(capturedArgs[1]);
    }

    [Fact]
    public async Task ProgressMetrics_ChartInitialization_PassesDataArray()
    {
        // Arrange
        object[] capturedArgs = null;
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) => capturedArgs = args)
            .ReturnsAsync(true);

        Services.AddScoped(_ => jsRuntimeMock.Object);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        await component.Instance.Task;

        // Assert
        Assert.NotNull(capturedArgs);
        Assert.True(capturedArgs.Length >= 3);
        Assert.NotNull(capturedArgs[2]);
    }

    [Fact]
    public async Task ProgressMetrics_ChartInitialization_Failure_DisplaysFallbackTable()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()))
            .ReturnsAsync(false);

        Services.AddScoped(_ => jsRuntimeMock.Object);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        await component.Instance.Task;

        // Assert
        Assert.Contains("Chart.js unavailable", component.Markup);
        Assert.Contains("table", component.Markup);
    }

    [Fact]
    public async Task ProgressMetrics_ChartInitialization_Failure_DisplaysTable()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock
            .Setup(x => x.InvokeAsync<bool>("ChartInterop.initializeBurndownChart", 
                It.IsAny<object[]>()))
            .ThrowsAsync(new Exception("Chart initialization failed"));

        Services.AddScoped(_ => jsRuntimeMock.Object);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        await component.Instance.Task;

        // Assert - Component should handle exception gracefully
        Assert.Contains("table", component.Markup);
    }
}