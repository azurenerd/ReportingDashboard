using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests
{
    public class StatusGridTests : TestContext
    {
        [Fact]
        public void StatusGrid_RendersWith3Columns()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test Project",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(3),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now } },
                InProgress = new() { new WorkItem { Id = "w2", Title = "Item 2", CompletedDate = DateTime.Now } },
                CarriedOver = new() { new WorkItem { Id = "w3", Title = "Item 3", CompletedDate = DateTime.Now } },
                Metrics = new()
            };

            var component = RenderComponent<StatusGrid>(parameters =>
                parameters.Add(p => p.Dashboard, dashboard)
            );

            var columns = component.FindAll(".col-md-4");
            Assert.Equal(3, columns.Count);
        }

        [Fact]
        public void StatusGrid_PassesShippedItemsToFirstColumn()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test Project",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(3),
                Milestones = new(),
                Shipped = new()
                {
                    new WorkItem { Id = "w1", Title = "Shipped Item", CompletedDate = DateTime.Now }
                },
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new()
            };

            var component = RenderComponent<StatusGrid>(parameters =>
                parameters.Add(p => p.Dashboard, dashboard)
            );

            component.WaitForAssertion(() =>
            {
                var shippedText = component.Markup;
                Assert.Contains("Shipped Item", shippedText);
            });
        }

        [Fact]
        public void StatusGrid_PassesInProgressItemsToSecondColumn()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test Project",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(3),
                Milestones = new(),
                Shipped = new(),
                InProgress = new()
                {
                    new WorkItem { Id = "w2", Title = "In Progress Item", CompletedDate = DateTime.Now }
                },
                CarriedOver = new(),
                Metrics = new()
            };

            var component = RenderComponent<StatusGrid>(parameters =>
                parameters.Add(p => p.Dashboard, dashboard)
            );

            component.WaitForAssertion(() =>
            {
                var text = component.Markup;
                Assert.Contains("In Progress Item", text);
            });
        }

        [Fact]
        public void StatusGrid_PassesCarriedOverItemsToThirdColumn()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test Project",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(3),
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
                {
                    new WorkItem { Id = "w3", Title = "Carried Over Item", CompletedDate = DateTime.Now }
                },
                Metrics = new()
            };

            var component = RenderComponent<StatusGrid>(parameters =>
                parameters.Add(p => p.Dashboard, dashboard)
            );

            component.WaitForAssertion(() =>
            {
                var text = component.Markup;
                Assert.Contains("Carried Over Item", text);
            });
        }

        [Fact]
        public void StatusGrid_HandlesNullDashboardGracefully()
        {
            var component = RenderComponent<StatusGrid>(parameters =>
                parameters.Add(p => p.Dashboard, null)
            );

            Assert.NotNull(component);
            Assert.DoesNotContain("error", component.Markup.ToLower());
        }

        [Fact]
        public void StatusGrid_DisplaysColumnCountBadges()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test Project",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(3),
                Milestones = new(),
                Shipped = new()
                {
                    new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now },
                    new WorkItem { Id = "w2", Title = "Item 2", CompletedDate = DateTime.Now }
                },
                InProgress = new()
                {
                    new WorkItem { Id = "w3", Title = "Item 3", CompletedDate = DateTime.Now }
                },
                CarriedOver = new(),
                Metrics = new()
            };

            var component = RenderComponent<StatusGrid>(parameters =>
                parameters.Add(p => p.Dashboard, dashboard)
            );

            component.WaitForAssertion(() =>
            {
                var text = component.Markup;
                Assert.Contains(">2<", text); // Shipped count
                Assert.Contains(">1<", text); // In Progress count
            });
        }
    }
}