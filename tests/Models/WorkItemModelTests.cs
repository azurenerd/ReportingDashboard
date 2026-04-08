using Xunit;
using AgentSquad.Runner.Models;
using System.Text.Json;

namespace AgentSquad.Tests.Models
{
    public class WorkItemModelTests
    {
        [Fact]
        public void WorkItemProperties_SetAndGetCorrectly()
        {
            var workItem = new WorkItem
            {
                Title = "API Integration",
                Description = "Build REST API endpoints",
                Status = WorkItemStatus.InProgress,
                AssignedTo = "Team A"
            };

            Assert.Equal("API Integration", workItem.Title);
            Assert.Equal("Build REST API endpoints", workItem.Description);
            Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
            Assert.Equal("Team A", workItem.AssignedTo);
        }

        [Fact]
        public void WorkItem_AllowsNullDescription()
        {
            var workItem = new WorkItem
            {
                Title = "Test Item",
                Description = null,
                Status = WorkItemStatus.Shipped,
                AssignedTo = null
            };

            Assert.Null(workItem.Description);
            Assert.Null(workItem.AssignedTo);
        }

        [Fact]
        public void WorkItem_AllowsEmptyStringDescription()
        {
            var workItem = new WorkItem
            {
                Title = "Test Item",
                Description = "",
                Status = WorkItemStatus.CarriedOver
            };

            Assert.Equal("", workItem.Description);
        }

        [Fact]
        public void WorkItemStatusEnum_HasAllRequiredValues()
        {
            Assert.True(System.Enum.IsDefined(typeof(WorkItemStatus), WorkItemStatus.Shipped));
            Assert.True(System.Enum.IsDefined(typeof(WorkItemStatus), WorkItemStatus.InProgress));
            Assert.True(System.Enum.IsDefined(typeof(WorkItemStatus), WorkItemStatus.CarriedOver));
        }

        [Fact]
        public void WorkItemStatusEnum_NumericValues_OrderedCorrectly()
        {
            Assert.Equal(0, (int)WorkItemStatus.Shipped);
            Assert.Equal(1, (int)WorkItemStatus.InProgress);
            Assert.Equal(2, (int)WorkItemStatus.CarriedOver);
        }

        [Fact]
        public void WorkItem_JsonSerialization_WithShippedStatus()
        {
            var workItem = new WorkItem
            {
                Title = "Completed Task",
                Description = "Task completed this month",
                Status = WorkItemStatus.Shipped,
                AssignedTo = "Team A"
            };

            var json = JsonSerializer.Serialize(workItem);
            Assert.Contains("\"title\":\"Completed Task\"", json);
            Assert.Contains("\"status\":\"Shipped\"", json);
        }

        [Fact]
        public void WorkItem_JsonDeserialization_WithInProgressStatus()
        {
            var json = @"{
                ""title"": ""In Progress Task"",
                ""description"": ""Currently being worked on"",
                ""status"": ""InProgress"",
                ""assignedTo"": ""Team B""
            }";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workItem = JsonSerializer.Deserialize<WorkItem>(json, options);

            Assert.NotNull(workItem);
            Assert.Equal("In Progress Task", workItem.Title);
            Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
            Assert.Equal("Team B", workItem.AssignedTo);
        }

        [Fact]
        public void WorkItem_JsonDeserialization_WithCarriedOverStatus()
        {
            var json = @"{
                ""title"": ""Carried Over Task"",
                ""description"": null,
                ""status"": ""CarriedOver"",
                ""assignedTo"": null
            }";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workItem = JsonSerializer.Deserialize<WorkItem>(json, options);

            Assert.NotNull(workItem);
            Assert.Equal("Carried Over Task", workItem.Title);
            Assert.Equal(WorkItemStatus.CarriedOver, workItem.Status);
            Assert.Null(workItem.AssignedTo);
            Assert.Null(workItem.Description);
        }

        [Fact]
        public void WorkItem_CanBeCastToInt_ForComparison()
        {
            Assert.Equal(0, (int)WorkItemStatus.Shipped);
            Assert.True((int)WorkItemStatus.Shipped < (int)WorkItemStatus.InProgress);
            Assert.True((int)WorkItemStatus.InProgress < (int)WorkItemStatus.CarriedOver);
        }

        [Fact]
        public void WorkItem_LongTitle_AcceptedWithoutTruncation()
        {
            var longTitle = new string('x', 500);
            var workItem = new WorkItem
            {
                Title = longTitle,
                Description = "Test",
                Status = WorkItemStatus.Shipped
            };

            Assert.Equal(longTitle, workItem.Title);
        }

        [Fact]
        public void WorkItem_LongDescription_AcceptedWithoutTruncation()
        {
            var longDescription = new string('y', 1000);
            var workItem = new WorkItem
            {
                Title = "Test",
                Description = longDescription,
                Status = WorkItemStatus.InProgress
            };

            Assert.Equal(longDescription, workItem.Description);
        }

        [Fact]
        public void WorkItemStatus_StringConversion_ReturnsCorrectEnumName()
        {
            Assert.Equal("Shipped", WorkItemStatus.Shipped.ToString());
            Assert.Equal("InProgress", WorkItemStatus.InProgress.ToString());
            Assert.Equal("CarriedOver", WorkItemStatus.CarriedOver.ToString());
        }
    }
}