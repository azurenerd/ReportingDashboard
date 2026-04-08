using Xunit;
using AgentSquad.Data;

namespace AgentSquad.Tests.Data
{
    public class DataModelTests
    {
        [Fact]
        public void ProjectInfo_CanBeInstantiatedWithRequiredProperties()
        {
            var project = new ProjectInfo { Name = "Test", Description = "Test Project" };
            Assert.Equal("Test", project.Name);
            Assert.Equal("Test Project", project.Description);
        }

        [Fact]
        public void Milestone_UsesTargetDateProperty()
        {
            var targetDate = new DateTime(2026, 12, 31);
            var milestone = new Milestone { Id = "m1", Name = "Phase 1", TargetDate = targetDate };
            Assert.Equal("m1", milestone.Id);
            Assert.Equal("Phase 1", milestone.Name);
            Assert.Equal(targetDate, milestone.TargetDate);
        }

        [Fact]
        public void Milestone_IdCanBeNullable()
        {
            var milestone = new Milestone { Id = null, Name = "Phase 1", TargetDate = DateTime.Now };
            Assert.Null(milestone.Id);
            Assert.Equal("Phase 1", milestone.Name);
        }

        [Fact]
        public void Task_UsesNamePropertyNotTitle()
        {
            var task = new Task { Id = "t1", Name = "Implementation", Status = "In Progress", AssignedTo = "Developer", DueDate = DateTime.Now };
            Assert.Equal("t1", task.Id);
            Assert.Equal("Implementation", task.Name);
            Assert.Equal("In Progress", task.Status);
        }

        [Fact]
        public void Task_ContainsRequiredProperties()
        {
            var dueDate = DateTime.Now.AddDays(7);
            var task = new Task 
            { 
                Id = "t1", 
                Name = "Code Review", 
                Status = "Complete",
                AssignedTo = "Alice",
                DueDate = dueDate
            };
            
            Assert.Equal("Code Review", task.Name);
            Assert.Equal("Complete", task.Status);
            Assert.Equal("Alice", task.AssignedTo);
            Assert.Equal(dueDate, task.DueDate);
        }

        [Fact]
        public void ProjectData_ContainsProjectAndMilestones()
        {
            var data = new ProjectData 
            { 
                Project = new ProjectInfo { Name = "Test" },
                Milestones = new List<Milestone>()
            };
            
            Assert.NotNull(data.Project);
            Assert.Equal("Test", data.Project.Name);
            Assert.NotNull(data.Milestones);
        }

        [Fact]
        public void Milestone_WithDifferentTargetDates_StoresSeparately()
        {
            var date1 = new DateTime(2026, 3, 31);
            var date2 = new DateTime(2026, 6, 30);
            
            var m1 = new Milestone { Id = "m1", Name = "Q1", TargetDate = date1 };
            var m2 = new Milestone { Id = "m2", Name = "Q2", TargetDate = date2 };
            
            Assert.NotEqual(m1.TargetDate, m2.TargetDate);
        }
    }
}