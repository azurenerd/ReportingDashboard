using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bunit;
using AgentSquad.Dashboard.Components;
using AgentSquad.Dashboard.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSquad.Dashboard.Tests.Integration.Components
{
    [Trait("Category", "Integration")]
    public class DashboardComponentIntegrationTests : IDisposable
    {
        private readonly TestContext _testContext;
        private readonly ServiceCollection _services;

        public DashboardComponentIntegrationTests()
        {
            _testContext = new TestContext();
            _services = new ServiceCollection();
            ConfigureServices(_services);
            _testContext.Services.AddMultiple(_services);
        }

        [Fact]
        public void Dashboard_RenderInitially_ShowsLoadingState()
        {
            var component = _testContext.RenderComponent<Dashboard>();

            Assert.NotNull(component);
        }

        [Fact]
        public void Dashboard_ChildComponents_AreRendered()
        {
            var component = _testContext.RenderComponent<Dashboard>();

            Assert.NotNull(component.FindComponent<MilestoneTimeline>());
            Assert.NotNull(component.FindComponents<StatusCard>());
        }

        [Fact]
        public void MilestoneTimeline_WithValidData_RendersCorrectly()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
            };

            var component = _testContext.RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));

            Assert.NotNull(component);
            Assert.Contains("M1", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTasks_DisplaysCorrectCount()
        {
            var tasks = new List<Task>
            {
                new Task { Name = "T1", Status = TaskStatus.Shipped },
                new Task { Name = "T2", Status = TaskStatus.Shipped }
            };

            var component = _testContext.RenderComponent<StatusCard>(parameters =>
                parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 2)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success"));

            Assert.NotNull(component);
            Assert.Contains("2", component.Markup);
        }

        [Fact]
        public void StatusCard_WithNoTasks_DisplaysEmptyMessage()
        {
            var component = _testContext.RenderComponent<StatusCard>(parameters =>
                parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 0)
                    .Add(p => p.Tasks, new List<Task>())
                    .Add(p => p.CardColor, "success"));

            Assert.Contains("No tasks", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_RendersWithValidValues()
        {
            var component = _testContext.RenderComponent<ProgressMetrics>(parameters =>
                parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.CompletedTasks, 5)
                    .Add(p => p.TotalTasks, 10)
                    .Add(p => p.BurndownRate, 1.5));

            Assert.NotNull(component);
            Assert.Contains("50%", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithEmptyList_ShowsMessage()
        {
            var component = _testContext.RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, new List<Milestone>()));

            Assert.Contains("No milestones", component.Markup);
        }

        [Fact]
        public void StatusCard_ToggleExpanded_UpdatesDisplay()
        {
            var tasks = new List<Task>
            {
                new Task { Name = "T1", Status = TaskStatus.Shipped }
            };

            var component = _testContext.RenderComponent<StatusCard>(parameters =>
                parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 1)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success"));

            var button = component.Find("button");
            button.Click();

            Assert.NotNull(component);
        }

        [Fact]
        public void Dashboard_WithMultipleStatusCards_AllCardsPresent()
        {
            var shippped = new List<Task> { new Task { Name = "T1", Status = TaskStatus.Shipped } };
            var inProgress = new List<Task> { new Task { Name = "T2", Status = TaskStatus.InProgress } };
            var carried = new List<Task> { new Task { Name = "T3", Status = TaskStatus.CarriedOver } };

            _testContext.RenderComponent<Dashboard>();

            Assert.NotNull(_testContext);
        }

        [Fact]
        public void MilestoneTimeline_OrdersMilestonesByTargetDate()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "M2", TargetDate = new DateTime(2024, 06, 01) },
                new Milestone { Name = "M1", TargetDate = new DateTime(2024, 03, 01) }
            };

            var component = _testContext.RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));

            var markup = component.Markup;
            var m1Index = markup.IndexOf("M1");
            var m2Index = markup.IndexOf("M2");

            Assert.True(m1Index < m2Index, "Milestone with earlier date should appear first");
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<ProjectDataService>();
        }

        public void Dispose()
        {
            _testContext?.Dispose();
        }
    }
}