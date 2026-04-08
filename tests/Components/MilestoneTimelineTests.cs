using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentSquad.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        private List<Milestone> CreateMilestones() => new List<Milestone>
        {
            new Milestone
            {
                Name = "Phase 1 Launch",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = "Core feature rollout"
            },
            new Milestone
            {
                Name = "Phase 2 Expansion",
                TargetDate = new DateTime(2026, 8, 30),
                Status = MilestoneStatus.InProgress,
                Description = "Extended functionality"
            },
            new Milestone
            {
                Name = "Phase 3 Optimization",
                TargetDate = new DateTime(2026, 10, 15),
                Status = MilestoneStatus.AtRisk,
                Description = "Performance improvements"
            },
            new Milestone
            {
                Name = "Phase 4 Release",
                TargetDate = new DateTime(2026, 12, 31),
                Status = MilestoneStatus.Future,
                Description = null
            }
        };

        [Fact]
        public void RendersWith_EmptyState_When_MilestonesNull()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, null));

            var emptyState = cut.Find(".timeline-empty-state");
            Assert.NotNull(emptyState);
            Assert.Contains("No milestones available", emptyState.TextContent);
        }

        [Fact]
        public void RendersWith_EmptyState_When_MilestonesEmpty()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone>()));

            var emptyState = cut.Find(".timeline-empty-state");
            Assert.NotNull(emptyState);
            Assert.Contains("No milestones available", emptyState.TextContent);
        }

        [Fact]
        public void Renders_TimelineContainer()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Test", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var container = cut.Find(".timeline-container");
            Assert.NotNull(container);
        }

        [Fact]
        public void Renders_SingleMilestone_WithAllProperties()
        {
            var milestone = new Milestone
            {
                Name = "Phase 1 Launch",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = "Core feature rollout"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.NotNull(item);
            Assert.Contains("Phase 1 Launch", item.TextContent);
            Assert.Contains("Jun 15, 2026", item.TextContent);
            Assert.Contains("Core feature rollout", item.TextContent);
        }

        [Fact]
        public void Renders_MultipleMilestones_WithCorrectCount()
        {
            var milestones = CreateMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var items = cut.FindAll(".timeline-item");
            Assert.Equal(4, items.Count);
        }

        [Fact]
        public void AppliesDataStatus_Completed()
        {
            var milestone = new Milestone
            {
                Name = "Completed Task",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("completed", item.GetAttribute("data-status"));
        }

        [Fact]
        public void AppliesDataStatus_InProgress()
        {
            var milestone = new Milestone
            {
                Name = "In Progress Task",
                TargetDate = new DateTime(2026, 7, 15),
                Status = MilestoneStatus.InProgress
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("inprogress", item.GetAttribute("data-status"));
        }

        [Fact]
        public void AppliesDataStatus_AtRisk()
        {
            var milestone = new Milestone
            {
                Name = "At Risk Task",
                TargetDate = new DateTime(2026, 8, 15),
                Status = MilestoneStatus.AtRisk
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("atrisk", item.GetAttribute("data-status"));
        }

        [Fact]
        public void AppliesDataStatus_Future()
        {
            var milestone = new Milestone
            {
                Name = "Future Task",
                TargetDate = new DateTime(2026, 12, 15),
                Status = MilestoneStatus.Future
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("future", item.GetAttribute("data-status"));
        }

        [Fact]
        public void FormatsDates_Correctly_MMM_DD_YYYY()
        {
            var testDates = new (DateTime date, string expected)[]
            {
                (new DateTime(2026, 1, 5), "Jan 05, 2026"),
                (new DateTime(2026, 6, 15), "Jun 15, 2026"),
                (new DateTime(2026, 12, 31), "Dec 31, 2026")
            };

            foreach (var (date, expected) in testDates)
            {
                var milestone = new Milestone
                {
                    Name = "Test",
                    TargetDate = date,
                    Status = MilestoneStatus.Completed
                };

                var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                    .Add(p => p.Milestones, new List<Milestone> { milestone }));

                var dateElement = cut.Find(".milestone-date");
                Assert.Contains(expected, dateElement.TextContent);
            }
        }

        [Fact]
        public void DoesNotRender_Description_WhenNull()
        {
            var milestone = new Milestone
            {
                Name = "No Description Task",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = null
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            var descriptions = item.QuerySelectorAll(".milestone-description");
            Assert.Empty(descriptions);
        }

        [Fact]
        public void DoesNotRender_Description_WhenEmpty()
        {
            var milestone = new Milestone
            {
                Name = "Empty Description Task",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = ""
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            var descriptions = item.QuerySelectorAll(".milestone-description");
            Assert.Empty(descriptions);
        }

        [Fact]
        public void Renders_MilestoneStatusBadge_ForEachMilestone()
        {
            var milestones = CreateMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var badges = cut.FindAll(".milestone-status-badge");
            Assert.Equal(4, badges.Count);
        }

        [Fact]
        public void Displays_AllMilestoneNames()
        {
            var milestones = CreateMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var names = cut.FindAll(".milestone-name");
            Assert.Equal(4, names.Count);
            Assert.Contains("Phase 1 Launch", names[0].TextContent);
            Assert.Contains("Phase 2 Expansion", names[1].TextContent);
            Assert.Contains("Phase 3 Optimization", names[2].TextContent);
            Assert.Contains("Phase 4 Release", names[3].TextContent);
        }

        [Fact]
        public void Handles_SpecialCharactersInMilestoneName()
        {
            var milestone = new Milestone
            {
                Name = "Q4 2026 - Major Release & Enhancement",
                TargetDate = new DateTime(2026, 9, 15),
                Status = MilestoneStatus.InProgress
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var nameElement = cut.Find(".milestone-name");
            Assert.Contains("Q4 2026 - Major Release & Enhancement", nameElement.TextContent);
        }

        [Fact]
        public void Handles_LongDescriptionText()
        {
            var longDescription = new string('x', 500);
            var milestone = new Milestone
            {
                Name = "Long Description Task",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = longDescription
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var descElement = cut.Find(".milestone-description");
            Assert.NotNull(descElement);
            Assert.Contains(longDescription, descElement.TextContent);
        }

        [Fact]
        public void Handles_MilestoneDateInPast()
        {
            var milestone = new Milestone
            {
                Name = "Past Milestone",
                TargetDate = new DateTime(2020, 1, 1),
                Status = MilestoneStatus.Completed
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var dateElement = cut.Find(".milestone-date");
            Assert.Contains("Jan 01, 2020", dateElement.TextContent);
        }

        [Fact]
        public void Handles_MilestoneDateInFarFuture()
        {
            var milestone = new Milestone
            {
                Name = "Far Future Milestone",
                TargetDate = new DateTime(2099, 12, 31),
                Status = MilestoneStatus.Future
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var dateElement = cut.Find(".milestone-date");
            Assert.Contains("Dec 31, 2099", dateElement.TextContent);
        }

        [Fact]
        public void PreservesRenderOrder_AsProvidedInList()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "First", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed },
                new Milestone { Name = "Second", TargetDate = DateTime.Now, Status = MilestoneStatus.InProgress },
                new Milestone { Name = "Third", TargetDate = DateTime.Now, Status = MilestoneStatus.Future }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var names = cut.FindAll(".milestone-name");
            Assert.Equal("First", names[0].TextContent.Trim());
            Assert.Equal("Second", names[1].TextContent.Trim());
            Assert.Equal("Third", names[2].TextContent.Trim());
        }

        [Fact]
        public void Renders_Correctly_WhenParameterUpdated()
        {
            var initialMilestones = new List<Milestone>
            {
                new Milestone { Name = "Initial", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, initialMilestones));

            Assert.Single(cut.FindAll(".timeline-item"));

            var updatedMilestones = new List<Milestone>
            {
                new Milestone { Name = "Initial", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed },
                new Milestone { Name = "Updated", TargetDate = DateTime.Now, Status = MilestoneStatus.InProgress }
            };

            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.Milestones, updatedMilestones));

            Assert.Equal(2, cut.FindAll(".timeline-item").Count);
        }

        [Fact]
        public void Renders_AllStatusTypes_InSingleList()
        {
            var milestones = CreateMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var items = cut.FindAll(".timeline-item");
            var statuses = items.Select(i => i.GetAttribute("data-status")).ToList();

            Assert.Contains("completed", statuses);
            Assert.Contains("inprogress", statuses);
            Assert.Contains("atrisk", statuses);
            Assert.Contains("future", statuses);
        }

        [Fact]
        public void DoesNotThrow_WithWhitespaceOnlyDescription()
        {
            var milestone = new Milestone
            {
                Name = "Whitespace Description",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = "   "
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.NotNull(item);
        }

        [Fact]
        public void Accessibility_TimelineContainsRequiredMarkup()
        {
            var milestones = CreateMilestones();
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var container = cut.Find(".timeline-container");
            Assert.NotNull(container);

            var items = cut.FindAll(".timeline-item");
            foreach (var item in items)
            {
                Assert.NotNull(item.GetAttribute("data-status"));
                var nameElement = item.QuerySelector(".milestone-name");
                var dateElement = item.QuerySelector(".milestone-date");
                Assert.NotNull(nameElement);
                Assert.NotNull(dateElement);
            }
        }

        [Fact]
        public void EmptyState_NotRendered_WhenMilestonesProvided()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Test", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var emptyStates = cut.FindAll(".timeline-empty-state");
            Assert.Empty(emptyStates);
        }

        [Fact]
        public void AllMilestonesRendered_WhenListExceedsScreenSize()
        {
            var milestones = new List<Milestone>();
            for (int i = 0; i < 20; i++)
            {
                milestones.Add(new Milestone
                {
                    Name = $"Milestone {i + 1}",
                    TargetDate = new DateTime(2026, 1, 1).AddMonths(i),
                    Status = (MilestoneStatus)(i % 4)
                });
            }

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var items = cut.FindAll(".timeline-item");
            Assert.Equal(20, items.Count);
        }
    }
}