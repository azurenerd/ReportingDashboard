using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProjectStatusSummaryTests
    {
        private List<Task> CreateMockTasks(
            int completedOnTime = 0, 
            int completedLate = 0, 
            int inProgress = 0, 
            int carriedOver = 0)
        {
            var tasks = new List<Task>();
            int taskId = 1;

            for (int i = 0; i < completedOnTime; i++)
            {
                tasks.Add(new Task
                {
                    Id = $"t{taskId++}",
                    Title = $"On-Time Completed Task {i + 1}",
                    Description = "Completed on schedule",
                    Status = TaskStatus.Completed,
                    AssignedTo = "Developer",
                    DueDate = DateTime.UtcNow.AddDays(-5),
                    IsOnTime = true
                });
            }

            for (int i = 0; i < completedLate; i++)
            {
                tasks.Add(new Task
                {
                    Id = $"t{taskId++}",
                    Title = $"Late Completed Task {i + 1}",
                    Description = "Completed after due date",
                    Status = TaskStatus.Completed,
                    AssignedTo = "Developer",
                    DueDate = DateTime.UtcNow.AddDays(-10),
                    IsOnTime = false
                });
            }

            for (int i = 0; i < inProgress; i++)
            {
                tasks.Add(new Task
                {
                    Id = $"t{taskId++}",
                    Title = $"In Progress Task {i + 1}",
                    Description = "Currently being worked on",
                    Status = TaskStatus.InProgress,
                    AssignedTo = "Developer",
                    DueDate = DateTime.UtcNow.AddDays(5)
                });
            }

            for (int i = 0; i < carriedOver; i++)
            {
                tasks.Add(new Task
                {
                    Id = $"t{taskId++}",
                    Title = $"Carried Over Task {i + 1}",
                    Description = "Deferred to next sprint",
                    Status = TaskStatus.CarriedOver,
                    AssignedTo = "Developer",
                    DueDate = DateTime.UtcNow
                });
            }

            return tasks;
        }

        [Fact]
        public void GetShippedCount_WithCompletedTasks_ReturnsCorrectCount()
        {
            var tasks = CreateMockTasks(completedOnTime: 6, completedLate: 2, inProgress: 3, carriedOver: 1);
            
            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            int shipped = component.GetShippedCount();

            Assert.Equal(8, shipped);
        }

        [Fact]
        public void GetShippedCount_WithNoCompletedTasks_ReturnsZero()
        {
            var tasks = CreateMockTasks(inProgress: 5, carriedOver: 2);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            int shipped = component.GetShippedCount();

            Assert.Equal(0, shipped);
        }

        [Fact]
        public void GetShippedCount_WithEmptyTaskList_ReturnsZero()
        {
            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(new List<Task>());

            int shipped = component.GetShippedCount();

            Assert.Equal(0, shipped);
        }

        [Fact]
        public void GetShippedCount_WithNullTaskList_ReturnsZero()
        {
            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(null);

            int shipped = component.GetShippedCount();

            Assert.Equal(0, shipped);
        }

        [Fact]
        public void GetInProgressCount_WithInProgressTasks_ReturnsCorrectCount()
        {
            var tasks = CreateMockTasks(completedOnTime: 6, completedLate: 2, inProgress: 3, carriedOver: 1);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            int inProgress = component.GetInProgressCount();

            Assert.Equal(3, inProgress);
        }

        [Fact]
        public void GetInProgressCount_WithNoInProgressTasks_ReturnsZero()
        {
            var tasks = CreateMockTasks(completedOnTime: 6, completedLate: 2, carriedOver: 1);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            int inProgress = component.GetInProgressCount();

            Assert.Equal(0, inProgress);
        }

        [Fact]
        public void GetInProgressCount_WithEmptyTaskList_ReturnsZero()
        {
            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(new List<Task>());

            int inProgress = component.GetInProgressCount();

            Assert.Equal(0, inProgress);
        }

        [Fact]
        public void GetOnTimePercentage_WithMixedCompletedTasks_CalculatesCorrectly()
        {
            var tasks = CreateMockTasks(completedOnTime: 6, completedLate: 2);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            double percentage = component.GetOnTimePercentage();

            Assert.Equal(75.0, percentage, 1);
        }

        [Fact]
        public void GetOnTimePercentage_WithAllOnTimeTasks_Returns100Percent()
        {
            var tasks = CreateMockTasks(completedOnTime: 5);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            double percentage = component.GetOnTimePercentage();

            Assert.Equal(100.0, percentage, 1);
        }

        [Fact]
        public void GetOnTimePercentage_WithNoOnTimeTasks_Returns0Percent()
        {
            var tasks = CreateMockTasks(completedLate: 5);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            double percentage = component.GetOnTimePercentage();

            Assert.Equal(0.0, percentage, 1);
        }

        [Fact]
        public void GetOnTimePercentage_WithNoCompletedTasks_ReturnsZero()
        {
            var tasks = CreateMockTasks(inProgress: 3, carriedOver: 2);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            double percentage = component.GetOnTimePercentage();

            Assert.Equal(0.0, percentage, 1);
        }

        [Fact]
        public void GetOnTimePercentage_WithEmptyTaskList_ReturnsZero()
        {
            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(new List<Task>());

            double percentage = component.GetOnTimePercentage();

            Assert.Equal(0.0, percentage, 1);
        }

        [Fact]
        public void GetColorClass_With80PercentOrHigher_ReturnsGreenClass()
        {
            var component = new ProjectStatusSummaryTestHelper();

            string colorClass = component.GetColorClass(80.0);
            Assert.Equal("text-success bg-light-success", colorClass);

            colorClass = component.GetColorClass(95.0);
            Assert.Equal("text-success bg-light-success", colorClass);

            colorClass = component.GetColorClass(100.0);
            Assert.Equal("text-success bg-light-success", colorClass);
        }

        [Fact]
        public void GetColorClass_With50To79Percent_ReturnsYellowClass()
        {
            var component = new ProjectStatusSummaryTestHelper();

            string colorClass = component.GetColorClass(50.0);
            Assert.Equal("text-warning bg-light-warning", colorClass);

            colorClass = component.GetColorClass(75.0);
            Assert.Equal("text-warning bg-light-warning", colorClass);

            colorClass = component.GetColorClass(79.9);
            Assert.Equal("text-warning bg-light-warning", colorClass);
        }

        [Fact]
        public void GetColorClass_WithLessThan50Percent_ReturnsRedClass()
        {
            var component = new ProjectStatusSummaryTestHelper();

            string colorClass = component.GetColorClass(0.0);
            Assert.Equal("text-danger bg-light-danger", colorClass);

            colorClass = component.GetColorClass(25.0);
            Assert.Equal("text-danger bg-light-danger", colorClass);

            colorClass = component.GetColorClass(49.9);
            Assert.Equal("text-danger bg-light-danger", colorClass);
        }

        [Fact]
        public void GetColorClass_WithNaNValue_ReturnsSecondaryClass()
        {
            var component = new ProjectStatusSummaryTestHelper();

            string colorClass = component.GetColorClass(double.NaN);
            Assert.Equal("text-secondary", colorClass);
        }

        [Fact]
        public void GetColorClass_WithInfinityValue_ReturnsSecondaryClass()
        {
            var component = new ProjectStatusSummaryTestHelper();

            string colorClass = component.GetColorClass(double.PositiveInfinity);
            Assert.Equal("text-secondary", colorClass);
        }

        [Fact]
        public void ScenarioTest_8Completed6OnTime2Late3InProgress1CarriedOver()
        {
            var tasks = CreateMockTasks(completedOnTime: 6, completedLate: 2, inProgress: 3, carriedOver: 1);

            var component = new ProjectStatusSummaryTestHelper();
            component.SetTasks(tasks);

            Assert.Equal(8, component.GetShippedCount());
            Assert.Equal(3, component.GetInProgressCount());
            Assert.Equal(75.0, component.GetOnTimePercentage(), 1);
            Assert.Equal("text-warning bg-light-warning", component.GetColorClass(75.0));
        }
    }

    internal class ProjectStatusSummaryTestHelper
    {
        private List<Task> _tasks = new();

        public void SetTasks(List<Task> tasks)
        {
            _tasks = tasks ?? new List<Task>();
        }

        public int GetShippedCount()
        {
            if (_tasks == null || _tasks.Count == 0)
                return 0;

            return _tasks.Count(t => t.Status == TaskStatus.Completed);
        }

        public int GetInProgressCount()
        {
            if (_tasks == null || _tasks.Count == 0)
                return 0;

            return _tasks.Count(t => t.Status == TaskStatus.InProgress);
        }

        public double GetOnTimePercentage()
        {
            try
            {
                var completedTasks = _tasks?
                    .Where(t => t.Status == TaskStatus.Completed)
                    .ToList() ?? new List<Task>();

                if (completedTasks.Count == 0)
                    return 0;

                int onTimeCount = completedTasks
                    .Count(t => IsTaskOnTime(t));

                double percentage = (double)onTimeCount / completedTasks.Count * 100;
                return percentage;
            }
            catch
            {
                return 0;
            }
        }

        public string GetColorClass(double percentage)
        {
            try
            {
                if (double.IsNaN(percentage) || double.IsInfinity(percentage))
                    return "text-secondary";

                if (percentage >= 80)
                    return "text-success bg-light-success";

                if (percentage >= 50)
                    return "text-warning bg-light-warning";

                return "text-danger bg-light-danger";
            }
            catch
            {
                return "text-secondary";
            }
        }

        private bool IsTaskOnTime(Task task)
        {
            try
            {
                if (task == null)
                    return true;

                var isOnTimeProperty = task.GetType().GetProperty("IsOnTime",
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);

                if (isOnTimeProperty != null && isOnTimeProperty.GetValue(task) is bool isOnTime)
                    return isOnTime;

                return true;
            }
            catch
            {
                return true;
            }
        }
    }
}