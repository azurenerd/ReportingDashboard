using Xunit;
using AgentSquad.Data;
using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Tests.Data
{
    public class DataModelEdgeCasesTests
    {
        [Fact]
        public void ProjectData_ValidatesCompletionPercentageRange()
        {
            var project = new ProjectData 
            { 
                Name = "Test", 
                CompletionPercentage = 50 
            };
            var results = new List<ValidationResult>();
            var context = new ValidationContext(project);
            
            Assert.True(Validator.TryValidateObject(project, context, results, true));
        }

        [Fact]
        public void ProjectData_RejectsCompletionPercentageAbove100()
        {
            var project = new ProjectData 
            { 
                Name = "Test", 
                CompletionPercentage = 101 
            };
            var results = new List<ValidationResult>();
            var context = new ValidationContext(project);
            
            Assert.False(Validator.TryValidateObject(project, context, results, true));
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ProjectData_RejectsNegativeCompletionPercentage()
        {
            var project = new ProjectData 
            { 
                Name = "Test", 
                CompletionPercentage = -1 
            };
            var results = new List<ValidationResult>();
            var context = new ValidationContext(project);
            
            Assert.False(Validator.TryValidateObject(project, context, results, true));
        }

        [Fact]
        public void Milestone_ValidatesTargetDateAfterStartDate()
        {
            var milestone = new Milestone
            {
                Name = "Test",
                StartDate = new DateTime(2026, 3, 1),
                TargetDate = new DateTime(2026, 5, 1)
            };
            var results = new List<ValidationResult>();
            var context = new ValidationContext(milestone);
            
            Assert.True(Validator.TryValidateObject(milestone, context, results, true));
        }

        [Fact]
        public void Milestone_RejectsTargetDateBeforeStartDate()
        {
            var milestone = new Milestone
            {
                Name = "Test",
                StartDate = new DateTime(2026, 5, 1),
                TargetDate = new DateTime(2026, 3, 1)
            };
            var results = new List<ValidationResult>();
            var context = new ValidationContext(milestone);
            
            Assert.False(Validator.TryValidateObject(milestone, context, results, true));
            Assert.NotEmpty(results);
        }

        [Fact]
        public void Task_AllowsNullOptionalFields()
        {
            var task = new ProjectTask 
            { 
                Name = "TestTask", 
                Description = null, 
                AssignedTo = null 
            };
            var results = new List<ValidationResult>();
            var context = new ValidationContext(task);
            
            Assert.True(Validator.TryValidateObject(task, context, results, true));
        }

        [Fact]
        public void ProjectMetrics_AllowsZeroCompletion()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 0 };
            Assert.Equal(0, metrics.CompletionPercentage);
        }

        [Fact]
        public void ProjectMetrics_AllowsFullCompletion()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 100 };
            Assert.Equal(100, metrics.CompletionPercentage);
        }
    }
}