using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Validators;

namespace AgentSquad.Runner.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class ProjectDashboardValidatorTests
    {
        [Fact]
        public void Validate_WithNullDashboard_ThrowsInvalidOperationException()
        {
            ProjectDashboard? dashboard = null;
            var ex = Assert.Throws<InvalidOperationException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("null", ex.Message.ToLower());
        }

        [Fact]
        public void Validate_WithNullProjectName_ThrowsJsonException()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = null!,
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("ProjectName", ex.Message);
        }

        [Fact]
        public void Validate_WithEmptyProjectName_ThrowsJsonException()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "",
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("ProjectName", ex.Message);
        }

        [Fact]
        public void Validate_WithWhitespaceProjectName_ThrowsJsonException()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "   ",
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("ProjectName", ex.Message);
        }

        [Fact]
        public void Validate_WithValidProjectName_PassesProjectNameCheck()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Project Alpha",
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_WithValidMilestoneStatus_Completed_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "Completed", TargetDate = DateTime.Now }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_WithValidMilestoneStatus_OnTrack_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "OnTrack", TargetDate = DateTime.Now }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_WithValidMilestoneStatus_AtRisk_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "AtRisk", TargetDate = DateTime.Now }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_WithValidMilestoneStatus_Delayed_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "Delayed", TargetDate = DateTime.Now }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_WithInvalidMilestoneStatus_ThrowsJsonException()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "InvalidStatus", TargetDate = DateTime.Now }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("Invalid milestone status", ex.Message);
            Assert.Contains("index 0", ex.Message);
        }

        [Fact]
        public void Validate_WithInvalidMilestoneStatus_IncludesIndex()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "Completed", TargetDate = DateTime.Now },
                    new() { Id = "m2", Name = "M2", Status = "OnTrack", TargetDate = DateTime.Now },
                    new() { Id = "m3", Name = "M3", Status = "BadStatus", TargetDate = DateTime.Now }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("index 2", ex.Message);
        }

        [Fact]
        public void Validate_WithDuplicateWorkItemId_Across_Shipped_And_InProgress_ThrowsJsonException()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new(),
                Shipped = new()
                {
                    new() { Id = "w001", Title = "Item 1" }
                },
                InProgress = new()
                {
                    new() { Id = "w001", Title = "Item 1 Duplicate" }
                },
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("duplicate", ex.Message.ToLower());
            Assert.Contains("w001", ex.Message);
        }

        [Fact]
        public void Validate_WithDuplicateWorkItemId_Across_Three_Columns_ThrowsJsonException()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new(),
                Shipped = new()
                {
                    new() { Id = "w001", Title = "Item 1" }
                },
                InProgress = new()
                {
                    new() { Id = "w002", Title = "Item 2" }
                },
                CarriedOver = new()
                {
                    new() { Id = "w001", Title = "Item 1 Duplicate" }
                }
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("duplicate", ex.Message.ToLower());
            Assert.Contains("w001", ex.Message);
        }

        [Fact]
        public void Validate_WithEmptyWorkItemId_ThrowsJsonException()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new(),
                Shipped = new()
                {
                    new() { Id = "", Title = "Item 1" }
                },
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("empty", ex.Message.ToLower());
        }

        [Fact]
        public void Validate_WithValidUniqueWorkItemIds_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new(),
                Shipped = new()
                {
                    new() { Id = "w001", Title = "Item 1" },
                    new() { Id = "w002", Title = "Item 2" }
                },
                InProgress = new()
                {
                    new() { Id = "w003", Title = "Item 3" }
                },
                CarriedOver = new()
                {
                    new() { Id = "w004", Title = "Item 4" }
                }
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_With45UniqueWorkItemIds_Across_45Items_Passes()
        {
            var shipped = Enumerable.Range(1, 15)
                .Select(i => new WorkItem { Id = $"w{i:D3}", Title = $"Item {i}" })
                .ToList();

            var inProgress = Enumerable.Range(16, 15)
                .Select(i => new WorkItem { Id = $"w{i:D3}", Title = $"Item {i}" })
                .ToList();

            var carriedOver = Enumerable.Range(31, 15)
                .Select(i => new WorkItem { Id = $"w{i:D3}", Title = $"Item {i}" })
                .ToList();

            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new(),
                Shipped = shipped,
                InProgress = inProgress,
                CarriedOver = carriedOver
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_WithCompleteValidDashboard_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Project Alpha",
                Description = "Test project",
                StartDate = new DateTime(2026, 1, 15),
                PlannedCompletion = new DateTime(2026, 6, 30),
                Milestones = new()
                {
                    new() { Id = "m1", Name = "Phase 1", Status = "Completed", TargetDate = new DateTime(2026, 2, 28) },
                    new() { Id = "m2", Name = "Phase 2", Status = "OnTrack", TargetDate = new DateTime(2026, 4, 15) },
                    new() { Id = "m3", Name = "Phase 3", Status = "AtRisk", TargetDate = new DateTime(2026, 5, 31) },
                    new() { Id = "m4", Name = "Phase 4", Status = "Delayed", TargetDate = new DateTime(2026, 6, 30) }
                },
                Shipped = new()
                {
                    new() { Id = "w1", Title = "Shipped Item 1" },
                    new() { Id = "w2", Title = "Shipped Item 2" }
                },
                InProgress = new()
                {
                    new() { Id = "w3", Title = "InProgress Item 1" }
                },
                CarriedOver = new()
                {
                    new() { Id = "w4", Title = "CarriedOver Item 1" }
                }
            };

            ProjectDashboardValidator.Validate(dashboard);
        }

        [Fact]
        public void Validate_WithNullMilestonesList_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = null!,
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("Milestones", ex.Message);
        }

        [Fact]
        public void Validate_WithNullShippedList_Passes()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new(),
                Shipped = null!,
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("Shipped", ex.Message);
        }

        [Fact]
        public void Validate_WithMixedValidAndInvalidMilestoneStatuses_ThrowsOnFirst()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "Completed", TargetDate = DateTime.Now },
                    new() { Id = "m2", Name = "M2", Status = "BadStatus", TargetDate = DateTime.Now },
                    new() { Id = "m3", Name = "M3", Status = "AnotherBadStatus", TargetDate = DateTime.Now }
                },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            var ex = Assert.Throws<System.Text.Json.JsonException>(() => ProjectDashboardValidator.Validate(dashboard));
            Assert.Contains("index 1", ex.Message);
            Assert.DoesNotContain("index 2", ex.Message);
        }
    }
}