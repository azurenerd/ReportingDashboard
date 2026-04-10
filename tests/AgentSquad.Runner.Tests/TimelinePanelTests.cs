using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests
{
    [Collection("Bunit")]
    public class TimelinePanelTests : TestContext
    {
        [Fact]
        public void Renders_Milestones_In_Chronological_Order()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Name = "Phase 2", TargetDate = new DateTime(2026, 03, 15), Status = "OnTrack" },
                    new() { Name = "Phase 1", TargetDate = new DateTime(2026, 02, 01), Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var names = cut.FindAll(".milestone-name").Select(x => x.TextContent).ToList();
            Assert.Equal(new[] { "Phase 1", "Phase 2" }, names);
        }

        [Fact]
        public void Renders_Status_Badge_With_Correct_Color_Class()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Name = "Test", TargetDate = DateTime.Now, Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.NotNull(cut.Find(".badge.bg-success"));
        }

        [Fact]
        public void Renders_Empty_State_When_No_Milestones()
        {
            var dashboard = new ProjectDashboard { Milestones = new() };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.Contains("No milestones to display", cut.Markup);
        }

        [Theory]
        [InlineData("completed", "Completed", "success")]
        [InlineData("ontrack", "On Track", "primary")]
        [InlineData("atrisk", "At Risk", "warning")]
        [InlineData("delayed", "Delayed", "danger")]
        public void Status_Color_Mapping_Correct(string statusInput, string expectedText, string expectedClass)
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Name = "Test", TargetDate = DateTime.Now, Status = statusInput }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var badge = cut.Find(".badge");
            Assert.Contains(expectedClass, badge.ClassName);
            Assert.Contains(expectedText, badge.TextContent);
        }

        [Fact]
        public void Renders_With_Sample_Dashboard_Data()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Project Alpha",
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Requirements", TargetDate = new(2026, 02, 28), Status = "Completed" },
                    new() { Id = "m002", Name = "Design", TargetDate = new(2026, 03, 31), Status = "OnTrack" },
                    new() { Id = "m003", Name = "Development", TargetDate = new(2026, 05, 15), Status = "AtRisk" },
                    new() { Id = "m004", Name = "Testing", TargetDate = new(2026, 06, 15), Status = "Delayed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.Equal(4, cut.FindAll(".timeline-item").Count);

            var names = cut.FindAll(".milestone-name").Select(x => x.TextContent).ToList();
            Assert.Equal(new[] { "Requirements", "Design", "Development", "Testing" }, names);

            Assert.Single(cut.FindAll(".bg-success"));
            Assert.Single(cut.FindAll(".bg-primary"));
            Assert.Single(cut.FindAll(".bg-warning"));
            Assert.Single(cut.FindAll(".bg-danger"));

            Assert.Contains("Feb 28, 2026", cut.Markup);
            Assert.Contains("Jun 15, 2026", cut.Markup);
        }

        [Fact]
        public void Renders_Aria_Labels_For_Status_Badges()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Name = "Requirements", TargetDate = new(2026, 02, 28), Status = "Completed" },
                    new() { Name = "Design", TargetDate = new(2026, 03, 31), Status = "OnTrack" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var badges = cut.FindAll(".badge");
            Assert.All(badges, badge =>
            {
                Assert.True(badge.HasAttribute("role"));
                Assert.Equal("status", badge.GetAttribute("role"));
                Assert.True(badge.HasAttribute("aria-label"));
            });
        }

        [Fact]
        public void Renders_Semantic_Markup()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Name = "Phase 1", TargetDate = DateTime.Now, Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var section = cut.Find("section");
            Assert.NotNull(section);
            Assert.True(section.HasAttribute("aria-label"));
            Assert.Equal("Project milestone timeline", section.GetAttribute("aria-label"));

            var hiddenHeading = cut.Find("h2.visually-hidden");
            Assert.NotNull(hiddenHeading);
            Assert.Equal("Milestones", hiddenHeading.TextContent);
        }

        [Fact]
        public void Milestone_Cards_Have_Correct_Structure()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Name = "Test Milestone", TargetDate = new(2026, 04, 15), Status = "OnTrack" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var item = cut.Find(".timeline-item");
            Assert.NotNull(item);

            var marker = item.QuerySelector(".timeline-marker");
            Assert.NotNull(marker);
            Assert.True(marker.HasAttribute("role"));
            Assert.Equal("presentation", marker.GetAttribute("role"));

            var content = item.QuerySelector(".timeline-content");
            Assert.NotNull(content);

            var name = content.QuerySelector(".milestone-name");
            Assert.NotNull(name);
            Assert.Equal("Test Milestone", name.TextContent);

            var date = content.QuerySelector(".milestone-date");
            Assert.NotNull(date);
            Assert.Contains("Apr 15, 2026", date.TextContent);

            var badge = content.QuerySelector(".badge");
            Assert.NotNull(badge);
            Assert.Contains("bg-primary", badge.ClassName);
            Assert.Contains("text-white", badge.ClassName);
        }

        [Fact]
        public void Handles_Case_Insensitive_Status_Values()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Name = "M1", TargetDate = DateTime.Now, Status = "COMPLETED" },
                    new() { Name = "M2", TargetDate = DateTime.Now.AddDays(1), Status = "OnTrack" },
                    new() { Name = "M3", TargetDate = DateTime.Now.AddDays(2), Status = "ATRISK" },
                    new() { Name = "M4", TargetDate = DateTime.Now.AddDays(3), Status = "delayed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var badges = cut.FindAll(".badge");
            Assert.Equal(4, badges.Count);
            Assert.Contains("bg-success", badges[0].ClassName);
            Assert.Contains("bg-primary", badges[1].ClassName);
            Assert.Contains("bg-warning", badges[2].ClassName);
            Assert.Contains("bg-danger", badges[3].ClassName);
        }

        [Fact]
        public void Null_Dashboard_Renders_Empty_State()
        {
            var cut = RenderComponent<TimelinePanel>();

            Assert.Contains("No milestones to display", cut.Markup);
        }
    }
}