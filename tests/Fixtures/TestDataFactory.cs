using AgentSquad.Models;

namespace AgentSquad.Tests.Fixtures
{
    public static class TestDataFactory
    {
        public static ProjectData CreateValidProjectData()
        {
            return new ProjectData
            {
                Project = new Project
                {
                    Name = "Test Project",
                    Description = "Test Description",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 6, 30)
                },
                Milestones = CreateTestMilestones(),
                Tasks = CreateTestTasks(),
                Metrics = CreateTestMetrics()
            };
        }

        public static List<Milestone> CreateTestMilestones()
        {
            return new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100,
                    TargetDate = new DateTime(2026, 1, 15)
                },
                new Milestone
                {
                    Name = "Phase 2",
                    Status = MilestoneStatus.InProgress,
                    CompletionPercentage = 50,
                    TargetDate = new DateTime(2026, 4, 1)
                },
                new Milestone
                {
                    Name = "Phase 3",
                    Status = MilestoneStatus.Pending,
                    CompletionPercentage = 0,
                    TargetDate = new DateTime(2026, 6, 30)
                }
            };
        }

        public static List<TaskItem> CreateTestTasks()
        {
            return new List<TaskItem>
            {
                new TaskItem { Id = "T1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Eng 1", DueDate = new DateTime(2026, 1, 15) },
                new TaskItem { Id = "T2", Name = "Task 2", Status = TaskStatus.InProgress, AssignedTo = "Eng 2", DueDate = new DateTime(2026, 4, 1) },
                new TaskItem { Id = "T3", Name = "Task 3", Status = TaskStatus.CarriedOver, AssignedTo = "Eng 3", DueDate = new DateTime(2026, 6, 1) }
            };
        }

        public static Metrics CreateTestMetrics()
        {
            return new Metrics
            {
                CompletionPercentage = 68,
                ShippedCount = 1,
                InProgressCount = 1,
                CarriedOverCount = 1,
                BurndownRate = 0.5m
            };
        }

        public static Project CreateTestProject()
        {
            return new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 6, 30)
            };
        }
    }
}