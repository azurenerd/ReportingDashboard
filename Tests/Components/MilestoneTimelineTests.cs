using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_RendersWith_EmptyState_When_MilestonesNull()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, null));

            cut.Find(".timeline-empty-state").Should().NotBeNull();
            cut.Find(".timeline-empty-state p").TextContent.Should().Contain("No milestones available");
        }

        [Fact]
        public void MilestoneTimeline_RendersWith_EmptyState_When_MilestonesEmpty()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone>()));

            cut.Find(".timeline-empty-state").Should().NotBeNull();
            cut.Find(".timeline-empty-state p").TextContent.Should().Contain("No milestones available");
        }

        [Fact]
        public void MilestoneTimeline_Renders_ContainerWithCorrectClass()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    TargetDate = new DateTime(2026, 06, 15),
                    Status = MilestoneStatus.Completed,
                    Description = "Core features"
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Find(".timeline-container").Should().NotBeNull();
            cut.FindAll(".timeline-item").Should().HaveCount(1);
        }

        [Fact]
        public void MilestoneTimeline_Displays_SingleMilestoneWithAllProperties()
        {
            var milestone = new Milestone
            {
                Name = "Phase 1 Launch",
                TargetDate = new DateTime(2026, 06, 15),
                Status = MilestoneStatus.Completed,
                Description = "Core feature rollout"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            item.Find(".milestone-name").TextContent.Should().Contain("Phase 1 Launch");
            item.Find(".milestone-date").TextContent.Should().Contain("Jun 15, 2026");
            item.Find(".milestone-description").TextContent.Should().Contain("Core feature rollout");
        }

        [Fact]
        public void MilestoneTimeline_AppliesCorrectDataStatus_ForCompleted()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Completed Task",
                    TargetDate = new DateTime(2026, 06, 15),
                    Status = MilestoneStatus.Completed,
                    Description = null
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var item = cut.Find(".timeline-item");
            item.GetAttribute("data-status").Should().Be("completed");
            item.Find(".milestone-status-badge").Should().NotBeNull();
        }

        [Fact]
        public void MilestoneTimeline_AppliesCorrectDataStatus_ForInProgress()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "In Progress Task",
                    TargetDate = new DateTime(2026, 07, 15),
                    Status = MilestoneStatus.InProgress,
                    Description = "Currently being worked on"
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var item = cut.Find(".timeline-item");
            item.GetAttribute("data-status").Should().Be("inprogress");
        }

        [Fact]
        public void MilestoneTimeline_AppliesCorrectDataStatus_ForAtRisk()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "At Risk Task",
                    TargetDate = new DateTime(2026, 08, 15),
                    Status = MilestoneStatus.AtRisk,
                    Description = "Potential issues ahead"
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var item = cut.Find(".timeline-item");
            item.GetAttribute("data-status").Should().Be("atrisk");
        }

        [Fact]
        public void MilestoneTimeline_AppliesCorrectDataStatus_ForFuture()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Future Task",
                    TargetDate = new DateTime(2026, 12, 15),
                    Status = MilestoneStatus.Future,
                    Description = "Planned for later"
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var item = cut.Find(".timeline-item");
            item.GetAttribute("data-status").Should().Be("future");
        }

        [Fact]
        public void MilestoneTimeline_FormatsDates_Correctly()
        {
            var milestone = new Milestone
            {
                Name = "Test Milestone",
                TargetDate = new DateTime(2026, 06, 15),
                Status = MilestoneStatus.Completed,
                Description = null
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var dateElement = cut.Find(".milestone-date");
            dateElement.TextContent.Should().Contain("Jun 15, 2026");
        }

        [Fact]
        public void MilestoneTimeline_DoesNotRender_DescriptionWhenNull()
        {
            var milestone = new Milestone
            {
                Name = "No Description Task",
                TargetDate = new DateTime(2026, 06, 15),
                Status = MilestoneStatus.Completed,
                Description = null
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            var descriptionElements = item.QuerySelectorAll(".milestone-description");
            descriptionElements.Should().HaveCount(0);
        }

        [Fact]
        public void MilestoneTimeline_Renders_MultipleStatusBadges()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Task 1",
                    TargetDate = new DateTime(2026, 06, 15),
                    Status = MilestoneStatus.Completed,
                    Description = null
                },
                new Milestone
                {
                    Name = "Task 2",
                    TargetDate = new DateTime(2026, 07, 15),
                    Status = MilestoneStatus.InProgress,
                    Description = null
                },
                new Milestone
                {
                    Name = "Task 3",
                    TargetDate = new DateTime(2026, 08, 15),
                    Status = MilestoneStatus.AtRisk,
                    Description = null
                },
                new Milestone
                {
                    Name = "Task 4",
                    TargetDate = new DateTime(2026, 12, 15),
                    Status = MilestoneStatus.Future,
                    Description = null
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.FindAll(".timeline-item").Should().HaveCount(4);
            cut.FindAll(".milestone-status-badge").Should().HaveCount(4);

            var items = cut.FindAll(".timeline-item");
            items[0].GetAttribute("data-status").Should().Be("completed");
            items[1].GetAttribute("data-status").Should().Be("inprogress");
            items[2].GetAttribute("data-status").Should().Be("atrisk");
            items[3].GetAttribute("data-status").Should().Be("future");
        }

        [Fact]
        public void MilestoneTimeline_AppliesCSSClasses_Correctly()
        {
            var milestone = new Milestone
            {
                Name = "CSS Test",
                TargetDate = new DateTime(2026, 06, 15),
                Status = MilestoneStatus.Completed,
                Description = "Testing CSS classes"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            cut.Find(".timeline-container").ClassName.Should().Contain("timeline-container");
            cut.Find(".timeline-item").ClassName.Should().Contain("timeline-item");
            cut.Find(".milestone-status-badge").ClassName.Should().Contain("milestone-status-badge");
            cut.Find(".milestone-name").ClassName.Should().Contain("milestone-name");
            cut.Find(".milestone-date").ClassName.Should().Contain("milestone-date");
            cut.Find(".milestone-description").ClassName.Should().Contain("milestone-description");
        }

        [Fact]
        public void MilestoneTimeline_Renders_WithoutConsoleErrors()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Valid Milestone",
                    TargetDate = new DateTime(2026, 06, 15),
                    Status = MilestoneStatus.Completed,
                    Description = "Valid description"
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            cut.Should().NotBeNull();
            cut.Instance.Should().NotBeNull();
        }

        [Fact]
        public void MilestoneTimeline_HandlesLongDescriptions_WithTruncation()
        {
            var longDescription = "This is a very long description that should be truncated in the timeline view. It contains multiple sentences and should not overflow the milestone item container. The CSS truncation should kick in and display ellipsis.";

            var milestone = new Milestone
            {
                Name = "Long Description Test",
                TargetDate = new DateTime(2026, 06, 15),
                Status = MilestoneStatus.Completed,
                Description = longDescription
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var description = cut.Find(".milestone-description");
            description.TextContent.Should().Contain("This is a very long description");
        }

        [Fact]
        public void MilestoneTimeline_Renders_AllMilestoneProperties_Correctly()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    TargetDate = new DateTime(2026, 03, 31),
                    Status = MilestoneStatus.Completed,
                    Description = "Phase 1 Launch"
                },
                new Milestone
                {
                    Name = "Phase 2",
                    TargetDate = new DateTime(2026, 06, 30),
                    Status = MilestoneStatus.InProgress,
                    Description = "Phase 2 Development"
                },
                new Milestone
                {
                    Name = "Phase 3",
                    TargetDate = new DateTime(2026, 09, 30),
                    Status = MilestoneStatus.Future,
                    Description = null
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var items = cut.FindAll(".timeline-item");
            items.Should().HaveCount(3);

            items[0].Find(".milestone-name").TextContent.Should().Contain("Phase 1");
            items[0].Find(".milestone-date").TextContent.Should().Contain("Mar 31, 2026");
            items[0].GetAttribute("data-status").Should().Be("completed");

            items[1].Find(".milestone-name").TextContent.Should().Contain("Phase 2");
            items[1].Find(".milestone-date").TextContent.Should().Contain("Jun 30, 2026");
            items[1].GetAttribute("data-status").Should().Be("inprogress");

            items[2].Find(".milestone-name").TextContent.Should().Contain("Phase 3");
            items[2].Find(".milestone-date").TextContent.Should().Contain("Sep 30, 2026");
            items[2].GetAttribute("data-status").Should().Be("future");
        }

        [Fact]
        public void MilestoneTimeline_RespondsTo_ParameterChanges()
        {
            var initialMilestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Initial Milestone",
                    TargetDate = new DateTime(2026, 06, 15),
                    Status = MilestoneStatus.Completed,
                    Description = "Initial"
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, initialMilestones));

            cut.FindAll(".timeline-item").Should().HaveCount(1);

            var updatedMilestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Updated Milestone",
                    TargetDate = new DateTime(2026, 07, 15),
                    Status = MilestoneStatus.InProgress,
                    Description = "Updated"
                },
                new Milestone
                {
                    Name = "New Milestone",
                    TargetDate = new DateTime(2026, 08, 15),
                    Status = MilestoneStatus.Future,
                    Description = "New"
                }
            };

            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.Milestones, updatedMilestones));

            cut.FindAll(".timeline-item").Should().HaveCount(2);
        }
    }
}