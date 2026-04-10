using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class ProjectDashboardModelTests
    {
        [Fact]
        public void ProjectDashboard_HasDefaultCollections()
        {
            var dashboard = new ProjectDashboard();

            Assert.NotNull(dashboard.Milestones);
            Assert.NotNull(dashboard.Shipped);
            Assert.NotNull(dashboard.InProgress);
            Assert.NotNull(dashboard.CarriedOver);
            Assert.NotNull(dashboard.Metrics);
        }

        [Fact]
        public void ProjectDashboard_CanSetAndGetProjectName()
        {
            var dashboard = new ProjectDashboard { ProjectName = "Test Project" };

            Assert.Equal("Test Project", dashboard.ProjectName);
        }

        [Fact]
        public void ProjectDashboard_CanSetAndGetDescription()
        {
            var dashboard = new ProjectDashboard { Description = "Test Description" };

            Assert.Equal("Test Description", dashboard.Description);
        }

        [Fact]
        public void ProjectDashboard_CanSetAndGetStartDate()
        {
            var date = new DateTime(2026, 1, 15);
            var dashboard = new ProjectDashboard { StartDate = date };

            Assert.Equal(date, dashboard.StartDate);
        }

        [Fact]
        public void ProjectDashboard_CanSetAndGetPlannedCompletion()
        {
            var date = new DateTime(2026, 6, 30);
            var dashboard = new ProjectDashboard { PlannedCompletion = date };

            Assert.Equal(date, dashboard.PlannedCompletion);
        }

        [Fact]
        public void ProjectDashboard_CanAddMilestones()
        {
            var dashboard = new ProjectDashboard();
            dashboard.Milestones.Add(new Milestone { Id = "m1", Name = "Phase 1" });

            Assert.Single(dashboard.Milestones);
            Assert.Equal("m1", dashboard.Milestones[0].Id);
        }

        [Fact]
        public void ProjectDashboard_CanAddShippedItems()
        {
            var dashboard = new ProjectDashboard();
            dashboard.Shipped.Add(new WorkItem { Id = "w1", Title = "Item 1" });

            Assert.Single(dashboard.Shipped);
            Assert.Equal("w1", dashboard.Shipped[0].Id);
        }

        [Fact]
        public void ProjectDashboard_CanAddInProgressItems()
        {
            var dashboard = new ProjectDashboard();
            dashboard.InProgress.Add(new WorkItem { Id = "w2", Title = "Item 2" });

            Assert.Single(dashboard.InProgress);
            Assert.Equal("w2", dashboard.InProgress[0].Id);
        }

        [Fact]
        public void ProjectDashboard_CanAddCarriedOverItems()
        {
            var dashboard = new ProjectDashboard();
            dashboard.CarriedOver.Add(new WorkItem { Id = "w3", Title = "Item 3" });

            Assert.Single(dashboard.CarriedOver);
            Assert.Equal("w3", dashboard.CarriedOver[0].Id);
        }
    }

    [Trait("Category", "Unit")]
    public class MilestoneModelTests
    {
        [Fact]
        public void Milestone_CanSetAndGetId()
        {
            var milestone = new Milestone { Id = "m001" };

            Assert.Equal("m001", milestone.Id);
        }

        [Fact]
        public void Milestone_CanSetAndGetName()
        {
            var milestone = new Milestone { Name = "Phase 1" };

            Assert.Equal("Phase 1", milestone.Name);
        }

        [Fact]
        public void Milestone_CanSetAndGetDescription()
        {
            var milestone = new Milestone { Description = "Test phase" };

            Assert.Equal("Test phase", milestone.Description);
        }

        [Fact]
        public void Milestone_CanSetAndGetTargetDate()
        {
            var date = new DateTime(2026, 2, 28);
            var milestone = new Milestone { TargetDate = date };

            Assert.Equal(date, milestone.TargetDate);
        }

        [Fact]
        public void Milestone_CanSetAndGetStatus()
        {
            var milestone = new Milestone { Status = "Completed" };

            Assert.Equal("Completed", milestone.Status);
        }

        [Fact]
        public void Milestone_HasDefaultEmptyStrings()
        {
            var milestone = new Milestone();

            Assert.Equal(string.Empty, milestone.Id);
            Assert.Equal(string.Empty, milestone.Name);
            Assert.Equal(string.Empty, milestone.Status);
        }
    }

    [Trait("Category", "Unit")]
    public class WorkItemModelTests
    {
        [Fact]
        public void WorkItem_CanSetAndGetId()
        {
            var item = new WorkItem { Id = "w001" };

            Assert.Equal("w001", item.Id);
        }

        [Fact]
        public void WorkItem_CanSetAndGetTitle()
        {
            var item = new WorkItem { Title = "Implement Feature" };

            Assert.Equal("Implement Feature", item.Title);
        }

        [Fact]
        public void WorkItem_CanSetAndGetDescription()
        {
            var item = new WorkItem { Description = "Feature description" };

            Assert.Equal("Feature description", item.Description);
        }

        [Fact]
        public void WorkItem_CanSetAndGetCompletedDate()
        {
            var date = new DateTime(2026, 2, 10);
            var item = new WorkItem { CompletedDate = date };

            Assert.Equal(date, item.CompletedDate);
        }

        [Fact]
        public void WorkItem_CompletedDateIsNullable()
        {
            var item = new WorkItem { CompletedDate = null };

            Assert.Null(item.CompletedDate);
        }

        [Fact]
        public void WorkItem_CanSetAndGetOwner()
        {
            var item = new WorkItem { Owner = "John Doe" };

            Assert.Equal("John Doe", item.Owner);
        }

        [Fact]
        public void WorkItem_HasDefaultEmptyStrings()
        {
            var item = new WorkItem();

            Assert.Equal(string.Empty, item.Id);
            Assert.Equal(string.Empty, item.Title);
        }
    }

    [Trait("Category", "Unit")]
    public class ProgressMetricsModelTests
    {
        [Fact]
        public void ProgressMetrics_CanSetAndGetTotalPlanned()
        {
            var metrics = new ProgressMetrics { TotalPlanned = 45 };

            Assert.Equal(45, metrics.TotalPlanned);
        }

        [Fact]
        public void ProgressMetrics_CanSetAndGetCompleted()
        {
            var metrics = new ProgressMetrics { Completed = 30 };

            Assert.Equal(30, metrics.Completed);
        }

        [Fact]
        public void ProgressMetrics_CanSetAndGetInFlight()
        {
            var metrics = new ProgressMetrics { InFlight = 15 };

            Assert.Equal(15, metrics.InFlight);
        }

        [Fact]
        public void ProgressMetrics_CanSetAndGetHealthScore()
        {
            var metrics = new ProgressMetrics { HealthScore = 66.67m };

            Assert.Equal(66.67m, metrics.HealthScore);
        }

        [Fact]
        public void ProgressMetrics_HasDefaultZeroValues()
        {
            var metrics = new ProgressMetrics();

            Assert.Equal(0, metrics.TotalPlanned);
            Assert.Equal(0, metrics.Completed);
            Assert.Equal(0, metrics.InFlight);
            Assert.Equal(0, metrics.HealthScore);
        }

        [Fact]
        public void ProgressMetrics_HealthScoreCanBe100()
        {
            var metrics = new ProgressMetrics { HealthScore = 100m };

            Assert.Equal(100m, metrics.HealthScore);
        }
    }
}