using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    /// <summary>
    /// Unit tests for MilestoneCalculations date calculations and status logic.
    /// Covers overdue detection, days remaining calculation, and edge cases.
    /// </summary>
    public class MilestoneCalculationsTests
    {
        /// <summary>
        /// Test: Null milestone throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithNullMilestone_ThrowsArgumentNullException()
        {
            // Arrange
            Milestone milestone = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                MilestoneCalculations.CalculateMilestoneViewModel(milestone));
            Assert.Equal("milestone", ex.ParamName);
        }

        /// <summary>
        /// Test: Milestone with default (empty) date throws ArgumentException.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithDefaultDate_ThrowsArgumentException()
        {
            // Arrange
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Test Milestone",
                Date = default(DateTime),
                Status = MilestoneStatus.Planned,
                Description = "Test"
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                MilestoneCalculations.CalculateMilestoneViewModel(milestone));
            Assert.Contains("date cannot be empty", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test: Future milestone (10 days) shows correct DaysRemaining and is not overdue.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithFutureDate_CalculatesDaysRemainingCorrectly()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(10);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Future Milestone",
                Date = futureDate,
                Status = MilestoneStatus.Planned,
                Description = "10 days from now"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.False(result.IsOverdue);
            Assert.InRange(result.DaysRemaining, 9.9, 10.1);
            Assert.Equal("Planned", result.StatusLabel);
            Assert.Equal("badge badge-secondary", result.CssClasses);
        }

        /// <summary>
        /// Test: Past milestone (5 days overdue) shows negative DaysRemaining and is overdue.
        /// Since >= 3 days overdue, marked as "At Risk" with red badge.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_With5DaysOverdue_MarkAsAtRisk()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-5);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Overdue Milestone",
                Date = pastDate,
                Status = MilestoneStatus.InProgress,
                Description = "5 days overdue"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.True(result.IsOverdue);
            Assert.InRange(result.DaysRemaining, -5.1, -4.9);
            Assert.Equal("At Risk", result.StatusLabel);
            Assert.Equal("badge badge-danger", result.CssClasses);
        }

        /// <summary>
        /// Test: Milestone exactly 3 days overdue is marked "At Risk".
        /// This is the threshold boundary condition.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_With3DaysOverdue_MarkAsAtRisk()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-3);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Boundary Milestone",
                Date = pastDate,
                Status = MilestoneStatus.Planned,
                Description = "Exactly 3 days overdue"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.True(result.IsOverdue);
            Assert.InRange(result.DaysRemaining, -3.1, -2.9);
            Assert.Equal("At Risk", result.StatusLabel);
            Assert.Equal("badge badge-danger", result.CssClasses);
        }

        /// <summary>
        /// Test: Milestone 2.9 days overdue is NOT marked "At Risk" (below 3-day threshold).
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_With2DaysOverdue_DoesNotMarkAsAtRisk()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-2).AddHours(-12);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Slightly Overdue",
                Date = pastDate,
                Status = MilestoneStatus.InProgress,
                Description = "Just under 3 days overdue"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.True(result.IsOverdue);
            Assert.InRange(result.DaysRemaining, -3.0, -2.5);
            Assert.Equal("In Progress", result.StatusLabel);
            Assert.Equal("badge badge-warning", result.CssClasses);
        }

        /// <summary>
        /// Test: Today's milestone (0 days remaining) is treated as overdue but not "At Risk".
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithTodayDate_IsNotAtRisk()
        {
            // Arrange
            var today = DateTime.UtcNow.Date.AddHours(12);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Today Milestone",
                Date = today,
                Status = MilestoneStatus.Completed,
                Description = "Due today"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.True(result.IsOverdue);
            Assert.InRange(result.DaysRemaining, -0.5, 0.5);
            Assert.Equal("Completed", result.StatusLabel);
            Assert.Equal("badge badge-success", result.CssClasses);
        }

        /// <summary>
        /// Test: Completed milestone renders with green badge regardless of date.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithCompletedStatus_RenderGreenBadge()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-10);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Completed Milestone",
                Date = pastDate,
                Status = MilestoneStatus.Completed,
                Description = "Completed 10 days ago"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.True(result.IsOverdue);
            Assert.Equal("Completed", result.StatusLabel);
            Assert.Equal("badge badge-success", result.CssClasses);
        }

        /// <summary>
        /// Test: DST transition date (Spring forward, 2026-03-08 02:00 EST -> 03:00 EDT).
        /// Verifies calculation across DST boundary.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithDSTSpringForwardDate_CalculatesCorrectly()
        {
            // Arrange - March 8, 2026 02:00 UTC (crosses DST boundary)
            var dstDate = new DateTime(2026, 3, 8, 7, 0, 0, DateTimeKind.Utc);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "DST Spring Forward",
                Date = dstDate,
                Status = MilestoneStatus.Planned,
                Description = "DST spring forward transition"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dstDate, result.Date);
            // DaysRemaining should be positive if dstDate is in future
            if (dstDate > DateTime.UtcNow)
                Assert.False(result.IsOverdue);
        }

        /// <summary>
        /// Test: DST transition date (Fall back, 2026-11-01 02:00 EDT -> 01:00 EST).
        /// Verifies calculation across fall-back DST boundary.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithDSTFallBackDate_CalculatesCorrectly()
        {
            // Arrange - November 1, 2026 06:00 UTC (crosses fall-back DST boundary)
            var dstDate = new DateTime(2026, 11, 1, 6, 0, 0, DateTimeKind.Utc);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "DST Fall Back",
                Date = dstDate,
                Status = MilestoneStatus.InProgress,
                Description = "DST fall back transition"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dstDate, result.Date);
            // Should calculate without error across DST boundary
            if (dstDate > DateTime.UtcNow)
                Assert.False(result.IsOverdue);
        }

        /// <summary>
        /// Test: AtRisk status enum displays "At Risk" label with red badge.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithAtRiskStatus_DisplaysAtRiskLabel()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(5);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "At Risk Milestone",
                Date = futureDate,
                Status = MilestoneStatus.AtRisk,
                Description = "Future milestone with at-risk status"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.Equal("At Risk", result.StatusLabel);
            Assert.Equal("badge badge-danger", result.CssClasses);
        }

        /// <summary>
        /// Test: InProgress status displays yellow badge with "In Progress" label.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithInProgressStatus_DisplaysYellowBadge()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(3);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "In Progress Milestone",
                Date = futureDate,
                Status = MilestoneStatus.InProgress,
                Description = "Currently in progress"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.Equal("In Progress", result.StatusLabel);
            Assert.Equal("badge badge-warning", result.CssClasses);
        }

        /// <summary>
        /// Test: Planned status displays gray badge.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithPlannedStatus_DisplaysSecondaryBadge()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(30);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Planned Milestone",
                Date = futureDate,
                Status = MilestoneStatus.Planned,
                Description = "Future planned milestone"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.Equal("Planned", result.StatusLabel);
            Assert.Equal("badge badge-secondary", result.CssClasses);
        }

        /// <summary>
        /// Test: Very far future milestone (365 days) displays correct days remaining.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_With365DaysRemaining_CalculatesCorrectly()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(365);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Far Future Milestone",
                Date = futureDate,
                Status = MilestoneStatus.Planned,
                Description = "365 days in the future"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.False(result.IsOverdue);
            Assert.InRange(result.DaysRemaining, 364.9, 365.1);
        }

        /// <summary>
        /// Test: Very far past milestone (365 days overdue) is marked At Risk.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_With365DaysOverdue_MarkAsAtRisk()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-365);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Far Past Milestone",
                Date = pastDate,
                Status = MilestoneStatus.Planned,
                Description = "365 days in the past"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.True(result.IsOverdue);
            Assert.InRange(result.DaysRemaining, -365.1, -364.9);
            Assert.Equal("At Risk", result.StatusLabel);
            Assert.Equal("badge badge-danger", result.CssClasses);
        }

        /// <summary>
        /// Test: Milestone with null description is handled gracefully.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithNullDescription_HandlesGracefully()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(10);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "No Description Milestone",
                Date = futureDate,
                Status = MilestoneStatus.Planned,
                Description = null
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Description);
            Assert.Equal("Planned", result.StatusLabel);
        }

        /// <summary>
        /// Test: Milestone with empty description string is handled gracefully.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_WithEmptyDescription_HandlesGracefully()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(10);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Empty Description Milestone",
                Date = futureDate,
                Status = MilestoneStatus.InProgress,
                Description = string.Empty
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Description);
            Assert.Equal("In Progress", result.StatusLabel);
        }

        /// <summary>
        /// Test: Properties are correctly copied from source milestone.
        /// </summary>
        [Fact]
        public void CalculateMilestoneViewModel_CopiesPropertiesCorrectly()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(7);
            var milestone = new Milestone
            {
                Id = "milestone-123",
                Name = "Q2 Infrastructure Setup",
                Date = futureDate,
                Status = MilestoneStatus.InProgress,
                Description = "Set up cloud infrastructure for Q2 project"
            };

            // Act
            var result = MilestoneCalculations.CalculateMilestoneViewModel(milestone);

            // Assert
            Assert.Equal("milestone-123", result.Id);
            Assert.Equal("Q2 Infrastructure Setup", result.Name);
            Assert.Equal(futureDate, result.Date);
            Assert.Equal(MilestoneStatus.InProgress, result.Status);
            Assert.Equal("Set up cloud infrastructure for Q2 project", result.Description);
        }
    }
}