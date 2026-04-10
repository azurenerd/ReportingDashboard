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
                    new() { Id = "m002", Name = "Phase 2", TargetDate = new DateTime(2026, 03, 15), Status = "OnTrack" },
                    new() { Id = "m001", Name = "Phase 1", TargetDate = new DateTime(2026, 02, 01), Status = "Completed" },
                    new() { Id = "m003", Name = "Phase 3", TargetDate = new DateTime(2026, 05, 30), Status = "AtRisk" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var names = cut.FindAll(".milestone-name").Select(x => x.TextContent.Trim()).ToList();
            Assert.Equal(3, names.Count);
            Assert.Equal("Phase 1", names[0]);
            Assert.Equal("Phase 2", names[1]);
            Assert.Equal("Phase 3", names[2]);
        }

        [Fact]
        public void Renders_Status_Badge_With_Correct_Color_Class()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Test", TargetDate = DateTime.Now, Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var badge = cut.Find(".badge");
            Assert.NotNull(badge);
            Assert.Contains("bg-success", badge.ClassName);
        }

        [Fact]
        public void Renders_Empty_State_When_No_Milestones()
        {
            var dashboard = new ProjectDashboard { Milestones = new() };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.Contains("No milestones", cut.Markup);
            Assert.Empty(cut.FindAll(".timeline-item"));
        }

        [Fact]
        public void Renders_Empty_State_When_Milestones_Null()
        {
            var dashboard = new ProjectDashboard { Milestones = null };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.Contains("No milestones", cut.Markup);
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
                    new() { Id = "m001", Name = "Test Milestone", TargetDate = DateTime.Now, Status = statusInput }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var badge = cut.Find(".badge");
            Assert.NotNull(badge);
            Assert.Contains(expectedClass, badge.ClassName);
            Assert.Contains(expectedText, badge.TextContent);
        }

        [Fact]
        public void Renders_Date_Formatted_Correctly()
        {
            var testDate = new DateTime(2026, 02, 28);
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Date Test", TargetDate = testDate, Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.Contains("Feb 28, 2026", cut.Markup);
        }

        [Fact]
        public void Renders_Multiple_Milestones_With_Different_Statuses()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Complete", TargetDate = new DateTime(2026, 01, 15), Status = "Completed" },
                    new() { Id = "m002", Name = "OnTrack", TargetDate = new DateTime(2026, 02, 15), Status = "OnTrack" },
                    new() { Id = "m003", Name = "AtRisk", TargetDate = new DateTime(2026, 03, 15), Status = "AtRisk" },
                    new() { Id = "m004", Name = "Delayed", TargetDate = new DateTime(2026, 04, 15), Status = "Delayed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.Equal(4, cut.FindAll(".timeline-item").Count);
            Assert.Single(cut.FindAll(".bg-success"));
            Assert.Single(cut.FindAll(".bg-primary"));
            Assert.Single(cut.FindAll(".bg-warning"));
            Assert.Single(cut.FindAll(".bg-danger"));
        }

        [Fact]
        public void Renders_With_Sample_Dashboard_Data()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Project Alpha",
                StartDate = new DateTime(2026, 01, 15),
                PlannedCompletion = new DateTime(2026, 06, 30),
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Requirements", TargetDate = new(2026, 02, 28), Status = "Completed", Description = "All requirements signed off" },
                    new() { Id = "m002", Name = "Design", TargetDate = new(2026, 03, 31), Status = "OnTrack", Description = "Design phase in progress" },
                    new() { Id = "m003", Name = "Development", TargetDate = new(2026, 05, 15), Status = "AtRisk", Description = "Development phase" },
                    new() { Id = "m004", Name = "Testing", TargetDate = new(2026, 06, 15), Status = "Delayed", Description = "Testing phase" }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new()
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            // Verify all 4 milestones render
            Assert.Equal(4, cut.FindAll(".timeline-item").Count);

            // Verify chronological order
            var names = cut.FindAll(".milestone-name").Select(x => x.TextContent.Trim()).ToList();
            Assert.Equal("Requirements", names[0]);
            Assert.Equal("Design", names[1]);
            Assert.Equal("Development", names[2]);
            Assert.Equal("Testing", names[3]);

            // Verify all status badges present with correct colors
            var badgesByClass = cut.FindAll(".badge");
            Assert.Equal(4, badgesByClass.Count);
            
            var successBadges = cut.FindAll(".bg-success");
            var primaryBadges = cut.FindAll(".bg-primary");
            var warningBadges = cut.FindAll(".bg-warning");
            var dangerBadges = cut.FindAll(".bg-danger");

            Assert.Single(successBadges);
            Assert.Single(primaryBadges);
            Assert.Single(warningBadges);
            Assert.Single(dangerBadges);

            // Verify formatted dates visible
            Assert.Contains("Feb 28, 2026", cut.Markup);
            Assert.Contains("Mar 31, 2026", cut.Markup);
            Assert.Contains("May 15, 2026", cut.Markup);
            Assert.Contains("Jun 15, 2026", cut.Markup);

            // Verify status text displays
            Assert.Contains("Completed", cut.Markup);
            Assert.Contains("On Track", cut.Markup);
            Assert.Contains("At Risk", cut.Markup);
            Assert.Contains("Delayed", cut.Markup);
        }

        [Fact]
        public void Renders_Semantic_Section_With_Aria_Label()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Test", TargetDate = DateTime.Now, Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var section = cut.Find("section");
            Assert.NotNull(section);
            Assert.Contains("aria-label", section.OuterHtml);
            Assert.Contains("Project milestone timeline", section.OuterHtml);
        }

        [Fact]
        public void Renders_Hidden_Milestone_Heading()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Test", TargetDate = DateTime.Now, Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var hiddenHeading = cut.Find("h2.visually-hidden");
            Assert.NotNull(hiddenHeading);
            Assert.Contains("Milestones", hiddenHeading.TextContent);
        }

        [Fact]
        public void Renders_Status_Badge_With_Role_Status()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Test", TargetDate = DateTime.Now, Status = "Completed" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var badge = cut.Find(".badge");
            Assert.NotNull(badge);
            Assert.Contains("role=\"status\"", badge.OuterHtml);
            Assert.Contains("aria-label", badge.OuterHtml);
        }

        [Fact]
        public void Handles_Case_Insensitive_Status_Values()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "Lowercase", TargetDate = DateTime.Now, Status = "completed" },
                    new() { Id = "m002", Name = "Uppercase", TargetDate = DateTime.Now.AddDays(1), Status = "ONTRACK" },
                    new() { Id = "m003", Name = "MixedCase", TargetDate = DateTime.Now.AddDays(2), Status = "AtRisk" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            Assert.Single(cut.FindAll(".bg-success"));
            Assert.Single(cut.FindAll(".bg-primary"));
            Assert.Single(cut.FindAll(".bg-warning"));
        }

        [Fact]
        public void Renders_Correct_Number_Of_Timeline_Items()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m001", Name = "M1", TargetDate = new DateTime(2026, 01, 01), Status = "Completed" },
                    new() { Id = "m002", Name = "M2", TargetDate = new DateTime(2026, 02, 01), Status = "OnTrack" },
                    new() { Id = "m003", Name = "M3", TargetDate = new DateTime(2026, 03, 01), Status = "AtRisk" },
                    new() { Id = "m004", Name = "M4", TargetDate = new DateTime(2026, 04, 01), Status = "Delayed" },
                    new() { Id = "m005", Name = "M5", TargetDate = new DateTime(2026, 05, 01), Status = "OnTrack" }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var timelineItems = cut.FindAll(".timeline-item");
            Assert.Equal(5, timelineItems.Count);
        }
    }
}