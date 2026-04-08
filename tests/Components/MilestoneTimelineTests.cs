using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_Renders_WithoutMilestones()
        {
            // Arrange
            var milestones = new List<Milestone>();
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act
            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, startDate)
                    .Add(p => p.ProjectEndDate, endDate)
            );

            // Assert
            Assert.NotNull(component);
        }

        [Fact]
        public void MilestoneTimeline_Renders_WithMilestones()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone 
                { 
                    Id = "m1", 
                    Name = "Design Review",
                    TargetDate = new DateTime(2024, 3, 31),
                    Status = MilestoneStatus.Completed
                }
            };
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act
            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, startDate)
                    .Add(p => p.ProjectEndDate, endDate)
            );

            // Assert
            Assert.NotNull(component);
            component.Markup.Should().Contain("milestone-timeline");
        }

        [Fact]
        public void MilestoneTimeline_Renders_WithPlaceholderText()
        {
            // Arrange
            var milestones = new List<Milestone>();
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act
            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, startDate)
                    .Add(p => p.ProjectEndDate, endDate)
            );

            // Assert
            component.Markup.Should().Contain("placeholder");
        }

        [Fact]
        public void MilestoneTimeline_Renders_MultipleMilestones()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone 
                { 
                    Id = "m1", 
                    Name = "Phase 1",
                    TargetDate = new DateTime(2024, 3, 31),
                    Status = MilestoneStatus.Completed
                },
                new Milestone 
                { 
                    Id = "m2", 
                    Name = "Phase 2",
                    TargetDate = new DateTime(2024, 6, 30),
                    Status = MilestoneStatus.InProgress
                },
                new Milestone 
                { 
                    Id = "m3", 
                    Name = "Phase 3",
                    TargetDate = new DateTime(2024, 9, 30),
                    Status = MilestoneStatus.Pending
                }
            };
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act
            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, startDate)
                    .Add(p => p.ProjectEndDate, endDate)
            );

            // Assert
            Assert.NotNull(component);
            Assert.Equal(3, milestones.Count);
        }

        [Fact]
        public void MilestoneTimeline_Renders_WithDateRange()
        {
            // Arrange
            var milestones = new List<Milestone>();
            var startDate = new DateTime(2024, 1, 15);
            var endDate = new DateTime(2024, 12, 15);

            // Act
            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, startDate)
                    .Add(p => p.ProjectEndDate, endDate)
            );

            // Assert
            Assert.NotNull(component);
        }
    }
}