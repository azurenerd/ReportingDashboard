using Xunit;
using AgentSquad.Data;

namespace AgentSquad.Tests.Data
{
    public class DataModelTests
    {
        [Fact]
        public void ProjectInfo_CanBeInstantiated()
        {
            var project = new ProjectInfo { Name = "Test", Description = "Test Project" };
            Assert.Equal("Test", project.Name);
            Assert.Equal("Test Project", project.Description);
        }

        [Fact]
        public void Milestone_CanBeInstantiated()
        {
            var milestone = new Milestone { Id = 1, Name = "Phase 1", DueDate = DateTime.Now };
            Assert.Equal(1, milestone.Id);
            Assert.Equal("Phase 1", milestone.Name);
        }

        [Fact]
        public void ProjectData_CanBeInstantiated()
        {
            var data = new ProjectData { Project = new ProjectInfo { Name = "Test" } };
            Assert.NotNull(data.Project);
            Assert.Equal("Test", data.Project.Name);
        }
    }
}