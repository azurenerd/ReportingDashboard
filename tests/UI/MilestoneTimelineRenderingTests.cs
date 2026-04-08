using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentSquad.Tests.UI
{
    public class MilestoneTimelineRenderingTests : TestContext
    {
        [Fact]
        public void Component_RendersWithoutErrors_WithValidData()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Test", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
            };

            var exception = Record.Exception(() =>
            {
                RenderComponent<MilestoneTimeline>(parameters => parameters
                    .Add(p => p.Milestones, milestones));
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Component_RendersWithoutErrors_WithEmptyList()
        {
            var exception = Record.Exception(() =>
            {
                RenderComponent<MilestoneTimeline>(parameters => parameters
                    .Add(p => p.Milestones, new List<Milestone>()));
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Component_RendersWithoutErrors_WithNullList()
        {
            var exception = Record.Exception(() =>
            {
                RenderComponent<MilestoneTimeline>(parameters => parameters
                    .Add(p => p.Milestones, null));
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Markup_ContainsTimelineContainer_Class()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone>()));

            var html = cut.Markup;
            Assert.Contains("timeline-container", html);
        }

        [Fact]
        public void Markup_ContainsCorrectDataAttributes_ForStatusElements()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Test", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var html = cut.Markup;
            Assert.Contains("data-status=", html);
        }

        [Fact]
        public void HTML_StructureIsValid_ForSingleMilestone()
        {
            var milestone = new Milestone
            {
                Name = "Test Milestone",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = "Test description"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            Assert.NotNull(cut.Find(".timeline-container"));
            Assert.NotNull(cut.Find(".timeline-item"));
            Assert.NotNull(cut.Find(".milestone-status-badge"));
            Assert.NotNull(cut.Find(".milestone-name"));
            Assert.NotNull(cut.Find(".milestone-date"));
            Assert.NotNull(cut.Find(".milestone-description"));
        }

        [Fact]
        public void HTML_StructureIsValid_WithoutDescription()
        {
            var milestone = new Milestone
            {
                Name = "Test Milestone",
                TargetDate = new DateTime(2026, 6, 15),
                Status = MilestoneStatus.Completed,
                Description = null
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            Assert.NotNull(cut.Find(".timeline-container"));
            Assert.NotNull(cut.Find(".timeline-item"));
            Assert.NotNull(cut.Find(".milestone-name"));
            Assert.NotNull(cut.Find(".milestone-date"));

            var descriptions = cut.FindAll(".milestone-description");
            Assert.Empty(descriptions);
        }

        [Fact]
        public void AllMilestones_AreRendered_InViewport()
        {
            var milestones = Enumerable.Range(1, 5)
                .Select(i => new Milestone
                {
                    Name = $"Milestone {i}",
                    TargetDate = DateTime.Now.AddMonths(i),
                    Status = (MilestoneStatus)(i % 4)
                }).ToList();

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var items = cut.FindAll(".timeline-item");
            Assert.Equal(5, items.Count);

            for (int i = 0; i < 5; i++)
            {
                Assert.Contains($"Milestone {i + 1}", items[i].TextContent);
            }
        }

        [Fact]
        public void MilestoneNames_AreVisible_AndReadable()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Phase 1", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed },
                new Milestone { Name = "Phase 2", TargetDate = DateTime.Now, Status = MilestoneStatus.InProgress },
                new Milestone { Name = "Phase 3", TargetDate = DateTime.Now, Status = MilestoneStatus.Future }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var names = cut.FindAll(".milestone-name");
            var nameTexts = names.Select(n => n.TextContent.Trim()).ToList();

            Assert.Contains("Phase 1", nameTexts);
            Assert.Contains("Phase 2", nameTexts);
            Assert.Contains("Phase 3", nameTexts);
        }

        [Fact]
        public void Dates_AreFormatted_Consistently()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", TargetDate = new DateTime(2026, 1, 1), Status = MilestoneStatus.Completed },
                new Milestone { Name = "M2", TargetDate = new DateTime(2026, 6, 15), Status = MilestoneStatus.InProgress },
                new Milestone { Name = "M3", TargetDate = new DateTime(2026, 12, 31), Status = MilestoneStatus.Future }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var dates = cut.FindAll(".milestone-date");
            Assert.All(dates, date =>
            {
                var text = date.TextContent;
                Assert.Matches(@"^[A-Z][a-z]{2} \d{2}, \d{4}$", text.Trim());
            });
        }

        [Fact]
        public void StatusBadges_ArePresent_ForEachMilestone()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed },
                new Milestone { Name = "M2", TargetDate = DateTime.Now, Status = MilestoneStatus.InProgress }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var badges = cut.FindAll(".milestone-status-badge");
            Assert.Equal(2, badges.Count);
        }

        [Fact]
        public void EmptyState_Message_IsDisplayed_Prominently()
        {
            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, null));

            var emptyState = cut.Find(".timeline-empty-state");
            var message = cut.Find(".timeline-empty-state p");

            Assert.NotNull(emptyState);
            Assert.NotNull(message);
            Assert.Contains("No milestones available", message.TextContent);
        }

        [Fact]
        public void Component_DoesNotRender_EmptyStateDiv_WhenMilestonesExist()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var emptyStates = cut.FindAll(".timeline-empty-state");
            Assert.Empty(emptyStates);
        }
    }
}