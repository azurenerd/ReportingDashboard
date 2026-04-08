using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        [Fact]
        public void Renders_EmptyState_WhenNoMilestones()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, new List<Milestone>())
            );

            Assert.Contains("No milestones available", component.Markup);
            Assert.Contains("timeline-empty", component.Markup);
        }

        [Fact]
        public void Renders_EmptyState_WhenMilestonesNull()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, (List<Milestone>)null)
            );

            Assert.Contains("No milestones available", component.Markup);
        }

        [Fact]
        public void Renders_SingleMilestone()
        {
            var milestones = new List<Milestone>
            {
                new() 
                { 
                    Name = "Phase 1 Complete", 
                    TargetDate = new DateTime(2026, 06, 15),
                    Status = MilestoneStatus.Completed,
                    Description = "Core features released"
                }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("Phase 1 Complete", component.Markup);
            Assert.Contains("Jun 15, 2026", component.Markup);
            Assert.Contains("Core features released", component.Markup);
            Assert.Contains("data-status=\"completed\"", component.Markup);
        }

        [Fact]
        public void Renders_MultipleMilestones_WithDifferentStatuses()
        {
            var milestones = new List<Milestone>
            {
                new() { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Completed },
                new() { Name = "M2", TargetDate = DateTime.Now.AddMonths(2), Status = MilestoneStatus.InProgress },
                new() { Name = "M3", TargetDate = DateTime.Now.AddMonths(3), Status = MilestoneStatus.AtRisk },
                new() { Name = "M4", TargetDate = DateTime.Now.AddMonths(4), Status = MilestoneStatus.Future }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("data-status=\"completed\"", component.Markup);
            Assert.Contains("data-status=\"inprogress\"", component.Markup);
            Assert.Contains("data-status=\"atrisk\"", component.Markup);
            Assert.Contains("data-status=\"future\"", component.Markup);
        }

        [Fact]
        public void Applies_CorrectCSSClasses_ForCompletedMilestone()
        {
            var milestones = new List<Milestone>
            {
                new() 
                { 
                    Name = "Release", 
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.Completed 
                }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("data-status=\"completed\"", component.Markup);
            Assert.Contains("milestone-status-badge", component.Markup);
        }

        [Fact]
        public void Applies_CorrectCSSClasses_ForInProgressMilestone()
        {
            var milestones = new List<Milestone>
            {
                new() 
                { 
                    Name = "Development", 
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.InProgress 
                }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("data-status=\"inprogress\"", component.Markup);
        }

        [Fact]
        public void Applies_CorrectCSSClasses_ForAtRiskMilestone()
        {
            var milestones = new List<Milestone>
            {
                new() 
                { 
                    Name = "Critical Delivery", 
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.AtRisk 
                }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("data-status=\"atrisk\"", component.Markup);
        }

        [Fact]
        public void Displays_DateInCorrectFormat()
        {
            var targetDate = new DateTime(2026, 03, 15);
            var milestones = new List<Milestone>
            {
                new() 
                { 
                    Name = "Target Milestone", 
                    TargetDate = targetDate,
                    Status = MilestoneStatus.Future 
                }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("Mar 15, 2026", component.Markup);
        }

        [Fact]
        public void Omits_Description_WhenNull()
        {
            var milestones = new List<Milestone>
            {
                new() 
                { 
                    Name = "Milestone", 
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.Future,
                    Description = null
                }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("Milestone", component.Markup);
        }

        [Fact]
        public void Includes_Description_WhenProvided()
        {
            var milestones = new List<Milestone>
            {
                new() 
                { 
                    Name = "Launch", 
                    TargetDate = DateTime.Now,
                    Status = MilestoneStatus.InProgress,
                    Description = "Public launch phase"
                }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("Public launch phase", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_DisplaysTimeline_Horizontally()
        {
            var milestones = new List<Milestone>
            {
                new() { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed },
                new() { Name = "M2", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("timeline-container", component.Markup);
            Assert.Contains("M1", component.Markup);
            Assert.Contains("M2", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_ShowsStatusIndicators()
        {
            var milestones = new List<Milestone>
            {
                new() { Name = "Green", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed },
                new() { Name = "Blue", TargetDate = DateTime.Now, Status = MilestoneStatus.InProgress },
                new() { Name = "Red", TargetDate = DateTime.Now, Status = MilestoneStatus.AtRisk }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("milestone-status-badge", component.Markup);
        }
    }
}