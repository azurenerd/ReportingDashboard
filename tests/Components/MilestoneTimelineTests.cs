using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        private List<Milestone> CreateTestMilestones()
        {
            return new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    TargetDate = DateTime.Now.AddMonths(1),
                    Status = MilestoneStatus.Completed,
                    Description = "Initial phase"
                },
                new Milestone
                {
                    Name = "Phase 2",
                    TargetDate = DateTime.Now.AddMonths(2),
                    Status = MilestoneStatus.InProgress,
                    Description = "Development phase"
                },
                new Milestone
                {
                    Name = "Phase 3",
                    TargetDate = DateTime.Now.AddMonths(3),
                    Status = MilestoneStatus.AtRisk,
                    Description = "Testing phase"
                },
                new Milestone
                {
                    Name = "Phase 4",
                    TargetDate = DateTime.Now.AddMonths(4),
                    Status = MilestoneStatus.Future,
                    Description = "Deployment phase"
                }
            };
        }

        [Fact]
        public void RenderMilestoneTimeline_WithValidMilestones()
        {
            var milestones = CreateTestMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            var timeline = cut.Find(".milestone-timeline");
            Assert.NotNull(timeline);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysTitle()
        {
            var milestones = CreateTestMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            var title = cut.Find(".timeline-title");
            Assert.Contains("Project Milestones", title.TextContent);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysAllMilestones()
        {
            var milestones = CreateTestMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            var items = cut.FindAll(".milestone-item");
            Assert.Equal(4, items.Count);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysMilestoneNames()
        {
            var milestones = CreateTestMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            Assert.Contains("Phase 1", cut.Markup);
            Assert.Contains("Phase 2", cut.Markup);
            Assert.Contains("Phase 3", cut.Markup);
            Assert.Contains("Phase 4", cut.Markup);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysMilestoneDates()
        {
            var milestones = CreateTestMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            var dateElements = cut.FindAll(".milestone-date");
            Assert.NotEmpty(dateElements);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysMilestoneDescriptions()
        {
            var milestones = CreateTestMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            Assert.Contains("Initial phase", cut.Markup);
            Assert.Contains("Development phase", cut.Markup);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysCompletedBadge()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Completed Milestone",
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.Completed
                }
            };
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            Assert.Contains("Completed", cut.Markup);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysInProgressBadge()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "In Progress Milestone",
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.InProgress
                }
            };
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            Assert.Contains("In Progress", cut.Markup);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysAtRiskBadge()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "At Risk Milestone",
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.AtRisk
                }
            };
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            Assert.Contains("At Risk", cut.Markup);
        }

        [Fact]
        public void RenderMilestoneTimeline_DisplaysFutureBadge()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Future Milestone",
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.Future
                }
            };
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            Assert.Contains("Future", cut.Markup);
        }

        [Fact]
        public void RenderMilestoneTimeline_DoesNotRender_WhenMilestonesNull()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, (List<Milestone>)null));

            cut.Render();
            var timeline = cut.QuerySelector(".milestone-timeline");
            Assert.Null(timeline);
        }

        [Fact]
        public void RenderMilestoneTimeline_DoesNotRender_WhenMilestonesEmpty()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone>()));

            cut.Render();
            var timeline = cut.QuerySelector(".milestone-timeline");
            Assert.Null(timeline);
        }

        [Fact]
        public void RenderMilestoneTimeline_SortsByTargetDate()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Third", TargetDate = DateTime.Now.AddMonths(3), Status = MilestoneStatus.Future },
                new Milestone { Name = "First", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Completed },
                new Milestone { Name = "Second", TargetDate = DateTime.Now.AddMonths(2), Status = MilestoneStatus.InProgress }
            };
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            var items = cut.FindAll(".milestone-name");
            Assert.Equal("First", items[0].TextContent);
            Assert.Equal("Second", items[1].TextContent);
            Assert.Equal("Third", items[2].TextContent);
        }

        [Fact]
        public void RenderMilestoneTimeline_MilestoneItemHasStatusAttribute()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Test",
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.Completed
                }
            };
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            var item = cut.Find(".milestone-item");
            var dataStatus = item.GetAttribute("data-status");
            Assert.Equal("completed", dataStatus);
        }

        [Fact]
        public void RenderMilestoneTimeline_NoDescription_StillRenders()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "No Description",
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.Completed,
                    Description = string.Empty
                }
            };
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Render();
            var item = cut.Find(".milestone-item");
            Assert.NotNull(item);
        }
    }
}