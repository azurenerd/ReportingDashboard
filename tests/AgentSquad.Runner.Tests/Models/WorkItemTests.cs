using AgentSquad.Runner.Models;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Models
{
    public class WorkItemTests
    {
        [Fact]
        public void WorkItem_DeserializesFromJson_WithAllFields()
        {
            var json = @"{
                ""title"": ""Implement Dashboard"",
                ""status"": ""In Progress"",
                ""assignedTo"": ""John Doe"",
                ""completionPercentage"": 60
            }";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workItem = JsonSerializer.Deserialize<WorkItem>(json, options);

            Assert.NotNull(workItem);
            Assert.Equal("Implement Dashboard", workItem.Title);
            Assert.Equal("In Progress", workItem.Status);
            Assert.Equal("John Doe", workItem.AssignedTo);
            Assert.Equal(60, workItem.CompletionPercentage);
        }

        [Fact]
        public void WorkItem_ValidatesCompletionPercentage_Between0And100()
        {
            var workItem = new WorkItem 
            { 
                Title = "Task", 
                Status = "Pending",
                CompletionPercentage = 75 
            };
            Assert.InRange(workItem.CompletionPercentage, 0, 100);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void WorkItem_InvalidCompletionPercentage_ThrowsException(int percentage)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                if (percentage < 0 || percentage > 100)
                    throw new ArgumentException("CompletionPercentage out of range");
            });
        }
    }
}