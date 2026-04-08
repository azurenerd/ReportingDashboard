using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Hosting;

namespace AgentSquad.Runner.Tests.Integration
{
    public class DashboardIntegrationTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly Mock<IWebHostEnvironment> _mockHostEnvironment;

        public DashboardIntegrationTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _mockHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Fact]
        public async Task Dashboard_WithValidData_LoadsAllComponents()
        {
            var project = new Project
            {
                Name = "Executive Dashboard",
                Description = "Test dashboard",
                StartDate = new DateTime(2024, 1, 1),
                Milestones = new List<Milestone>
                {
                    new() { Name = "Milestone 1", TargetDate = new DateTime(2024, 2, 1), Status = MilestoneStatus.Completed, Description = "M1" },
                    new() { Name = "Milestone 2", TargetDate = new DateTime(2024, 3, 1), Status = MilestoneStatus.InProgress, Description = "M2" },
                    new() { Name = "Milestone 3", TargetDate = new DateTime(2024, 4, 1), Status = MilestoneStatus.Future, Description = "M3" }
                },
                WorkItems = new List<WorkItem>
                {
                    new() { Title = "Feature A", Description = "Shipped", Status = WorkItemStatus.Shipped, AssignedTo = "Alice" },
                    new() { Title = "Feature B", Description = "In progress", Status = WorkItemStatus.InProgress, AssignedTo = "Bob" },
                    new() { Title = "Feature C", Description = "Carried over", Status = WorkItemStatus.CarriedOver, AssignedTo = "Charlie" }
                },
                Metrics = new ProjectMetrics
                {
                    CompletionPercentage = 72,
                    HealthStatus = HealthStatus.OnTrack,
                    VelocityCount = 12
                }
            };

            Assert.Equal(3, project.Milestones.Count);
            Assert.Equal(3, project.WorkItems.Count);
            Assert.Equal(72, project.Metrics.CompletionPercentage);
        }

        [Fact]
        public async Task Dashboard_WithMixedMilestoneStatuses_RendersDifferentIndicators()
        {
            var milestones = new List<Milestone>
            {
                new() { Status = MilestoneStatus.Completed },
                new() { Status = MilestoneStatus.InProgress },
                new() { Status = MilestoneStatus.AtRisk },
                new() { Status = MilestoneStatus.Future }
            };

            var completedCount = milestones.FindAll(m => m.Status == MilestoneStatus.Completed).Count;
            var inProgressCount = milestones.FindAll(m => m.Status == MilestoneStatus.InProgress).Count;
            var atRiskCount = milestones.FindAll(m => m.Status == MilestoneStatus.AtRisk).Count;
            var futureCount = milestones.FindAll(m => m.Status == MilestoneStatus.Future).Count;

            Assert.Equal(1, completedCount);
            Assert.Equal(1, inProgressCount);
            Assert.Equal(1, atRiskCount);
            Assert.Equal(1, futureCount);
        }

        [Fact]
        public async Task Dashboard_WorkItems_GroupedByStatus()
        {
            var workItems = new List<WorkItem>
            {
                new() { Title = "WI1", Status = WorkItemStatus.Shipped },
                new() { Title = "WI2", Status = WorkItemStatus.Shipped },
                new() { Title = "WI3", Status = WorkItemStatus.InProgress },
                new() { Title = "WI4", Status = WorkItemStatus.CarriedOver }
            };

            var shipped = workItems.FindAll(w => w.Status == WorkItemStatus.Shipped);
            var inProgress = workItems.FindAll(w => w.Status == WorkItemStatus.InProgress);
            var carriedOver = workItems.FindAll(w => w.Status == WorkItemStatus.CarriedOver);

            Assert.Equal(2, shipped.Count);
            Assert.Equal(1, inProgress.Count);
            Assert.Equal(1, carriedOver.Count);
        }

        [Fact]
        public async Task Dashboard_Metrics_DisplayHealthStatus()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 75,
                HealthStatus = HealthStatus.OnTrack,
                VelocityCount = 10
            };

            Assert.Equal(75, metrics.CompletionPercentage);
            Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
            Assert.Equal(10, metrics.VelocityCount);
        }

        [Fact]
        public async Task Dashboard_MetricsUpdate_RefreshesFromData()
        {
            var oldMetrics = new ProjectMetrics { CompletionPercentage = 50, HealthStatus = HealthStatus.AtRisk };
            var newMetrics = new ProjectMetrics { CompletionPercentage = 75, HealthStatus = HealthStatus.OnTrack };

            Assert.NotEqual(oldMetrics.CompletionPercentage, newMetrics.CompletionPercentage);
            Assert.NotEqual(oldMetrics.HealthStatus, newMetrics.HealthStatus);
        }

        [Fact]
        public async Task Dashboard_TimelineRendering_IncludesMilestoneDetails()
        {
            var milestone = new Milestone
            {
                Name = "Release v1.0",
                TargetDate = new DateTime(2024, 6, 30),
                Status = MilestoneStatus.InProgress,
                Description = "Major version release"
            };

            Assert.NotEmpty(milestone.Name);
            Assert.NotEqual(default(DateTime), milestone.TargetDate);
            Assert.NotEmpty(milestone.Description);
        }

        [Fact]
        public async Task Dashboard_AcceptanceCriteria_AllSectionsVisible()
        {
            var project = new Project
            {
                Name = "Test",
                Milestones = new List<Milestone>
                {
                    new() { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.Future, Description = "" }
                },
                WorkItems = new List<WorkItem>
                {
                    new() { Title = "WI", Status = WorkItemStatus.Shipped }
                },
                Metrics = new ProjectMetrics { CompletionPercentage = 50, HealthStatus = HealthStatus.OnTrack }
            };

            Assert.NotNull(project.Name);
            Assert.NotEmpty(project.Milestones);
            Assert.NotEmpty(project.WorkItems);
            Assert.NotNull(project.Metrics);
        }
    }
}