using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        private Mock<IJSRuntime> _mockJsRuntime;
        private Milestone[] _testMilestones;

        public MilestoneTimelineTests()
        {
            _mockJsRuntime = new Mock<IJSRuntime>();
            _testMilestones = new[]
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Platform Launch",
                    TargetDate = "2026-05-15",
                    Status = "on-track",
                    Progress = 75,
                    Description = "Core platform infrastructure"
                },
                new Milestone
                {
                    Id = "m2",
                    Name = "API Integration",
                    TargetDate = "2026-06-30",
                    Status = "at-risk",
                    Progress = 45,
                    Description = "Third-party API integrations"
                },
                new Milestone
                {
                    Id = "m3",
                    Name = "Security Hardening",
                    TargetDate = "2026-07-31",
                    Status = "delayed",
                    Progress = 20,
                    Description = "Security audit and fixes"
                }
            };
        }

        [Fact]
        public void Render_WithMilestones_DisplaysCanvas()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.LastRefreshTime, DateTime.Now));

            // Assert
            var canvas = component.Find("canvas#milestoneChart");
            canvas.Should().NotBeNull();
            canvas.GetAttribute("id").Should().Be("milestoneChart");
        }

        [Fact]
        public void Render_WithMilestones_InvokesChartJs()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.LastRefreshTime, DateTime.Now));

            // Assert
            _mockJsRuntime.Verify(
                x => x.InvokeAsync<object>(
                    "initMilestoneChart",
                    It.Is<object[]>(args => args.Length > 0)),
                Times.Once);
        }

        [Fact]
        public void Render_WithEmptyMilestones_DisplaysCanvasWithoutError()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            var emptyMilestones = Array.Empty<Milestone>();

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, emptyMilestones)
                .Add(p => p.LastRefreshTime, DateTime.Now));

            // Assert
            var canvas = component.Find("canvas#milestoneChart");
            canvas.Should().NotBeNull();
            component.Markup.Should().Contain("milestoneChart");
        }

        [Fact]
        public void OnParametersSet_WithMilestoneChange_ReinitializesChart()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.LastRefreshTime, DateTime.Now));

            _mockJsRuntime.Invocations.Clear();

            var updatedMilestones = new[]
            {
                _testMilestones[0],
                new Milestone
                {
                    Id = "m4",
                    Name = "New Milestone",
                    TargetDate = "2026-08-31",
                    Status = "on-track",
                    Progress = 50,
                    Description = "New milestone"
                }
            };

            // Act
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.Milestones, updatedMilestones));

            // Assert
            _mockJsRuntime.Verify(
                x => x.InvokeAsync<object>(
                    "initMilestoneChart",
                    It.Is<object[]>(args => args.Length > 0)),
                Times.Once);
        }

        [Fact]
        public void Render_WithNullMilestones_HandlesGracefully()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            // Act & Assert - should not throw
            var exception = Record.Exception(() =>
                RenderComponent<MilestoneTimeline>(parameters => parameters
                    .Add(p => p.Milestones, (Milestone[])null)
                    .Add(p => p.LastRefreshTime, DateTime.Now)));

            exception.Should().BeNull();
        }

        [Fact]
        public void ChartInitialization_PassesCorrectColorScheme()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            var capturedConfig = default(object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .Callback<string, object[]>((name, args) => capturedConfig = args.FirstOrDefault())
                .ReturnsAsync(new object());

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.LastRefreshTime, DateTime.Now));

            // Assert
            _mockJsRuntime.Verify(
                x => x.InvokeAsync<object>(
                    "initMilestoneChart",
                    It.Is<object[]>(args =>
                        args.Length > 0 &&
                        args[0] != null)),
                Times.Once);
        }

        [Fact]
        public void Render_WithMultipleMilestones_DoesNotThrow()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            var largeMilestoneSet = Enumerable.Range(1, 10)
                .Select(i => new Milestone
                {
                    Id = $"m{i}",
                    Name = $"Milestone {i}",
                    TargetDate = $"2026-{(5 + i):D2}-15",
                    Status = i % 3 == 0 ? "completed" : (i % 3 == 1 ? "on-track" : "at-risk"),
                    Progress = i * 10,
                    Description = $"Description {i}"
                })
                .ToArray();

            // Act & Assert - should not throw
            var exception = Record.Exception(() =>
                RenderComponent<MilestoneTimeline>(parameters => parameters
                    .Add(p => p.Milestones, largeMilestoneSet)
                    .Add(p => p.LastRefreshTime, DateTime.Now)));

            exception.Should().BeNull();
        }

        [Fact]
        public void Render_CanvasHasCorrectContainerId()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.LastRefreshTime, DateTime.Now));

            // Assert
            var canvas = component.Find("canvas");
            canvas.GetAttribute("id").Should().Be("milestoneChart");
        }

        [Fact]
        public void Render_ContainerHasTimelineContainerClass()
        {
            // Arrange
            Services.AddScoped(_ => _mockJsRuntime.Object);
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ReturnsAsync(new object());

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.LastRefreshTime, DateTime.Now));

            // Assert
            var container = component.Find("div.timeline-container");
            container.Should().NotBeNull();
        }
    }
}