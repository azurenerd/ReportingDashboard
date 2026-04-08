using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;

namespace AgentSquad.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        private readonly List<Milestone> _testMilestones;

        public MilestoneTimelineTests()
        {
            _testMilestones = new List<Milestone>
            {
                new Milestone { Name = "Phase 1", Status = MilestoneStatus.Completed, CompletionPercentage = 100, TargetDate = new DateTime(2026, 1, 15) },
                new Milestone { Name = "Phase 2", Status = MilestoneStatus.InProgress, CompletionPercentage = 50, TargetDate = new DateTime(2026, 4, 1) },
                new Milestone { Name = "Phase 3", Status = MilestoneStatus.Pending, CompletionPercentage = 0, TargetDate = new DateTime(2026, 6, 30) },
                new Milestone { Name = "Phase 4", Status = MilestoneStatus.Pending, CompletionPercentage = 0, TargetDate = new DateTime(2026, 9, 15) }
            };
        }

        [Fact]
        public void MilestoneTimeline_RendersMilestones()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            Assert.Contains("Project Timeline", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_DisplaysAllMilestoneNames()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            foreach (var milestone in _testMilestones)
            {
                Assert.Contains(milestone.Name, component.Markup);
            }
        }

        [Fact]
        public void MilestoneTimeline_DisplaysMilestoneDates()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            Assert.Contains("Jan 15, 2026", component.Markup);
            Assert.Contains("Apr 01, 2026", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_DisplaysCompletionPercentages()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            Assert.Contains("100%", component.Markup);
            Assert.Contains("50%", component.Markup);
            Assert.Contains("0%", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_ShowsMessageWhenNoMilestones()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone>())
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            Assert.Contains("No milestones available", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_AppliesStatusColors()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            Assert.Contains("#28a745", component.Markup);
            Assert.Contains("#007bff", component.Markup);
            Assert.Contains("#6c757d", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_FullWidthResponsive()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            var section = component.Find("section");
            Assert.NotNull(section);
            Assert.Contains("section-spacing", section.GetAttribute("class") ?? "");
        }

        [Fact]
        public void MilestoneTimeline_DisplaysTimelineContainer()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, _testMilestones)
                .Add(p => p.ProjectStartDate, new DateTime(2026, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2026, 9, 30)));

            var timeline = component.Find(".timeline");
            Assert.NotNull(timeline);
        }
    }
}