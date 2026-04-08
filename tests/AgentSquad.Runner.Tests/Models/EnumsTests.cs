using Xunit;
using AgentSquad.Runner.Models;
using System.Text.Json;

namespace AgentSquad.Runner.Tests.Models
{
    public class EnumsTests
    {
        [Fact]
        public void MilestoneStatus_HasCorrectValues()
        {
            Assert.Equal(0, (int)MilestoneStatus.Completed);
            Assert.Equal(1, (int)MilestoneStatus.InProgress);
            Assert.Equal(2, (int)MilestoneStatus.AtRisk);
            Assert.Equal(3, (int)MilestoneStatus.Future);
        }

        [Fact]
        public void WorkItemStatus_HasCorrectValues()
        {
            Assert.Equal(0, (int)WorkItemStatus.Shipped);
            Assert.Equal(1, (int)WorkItemStatus.InProgress);
            Assert.Equal(2, (int)WorkItemStatus.CarriedOver);
        }

        [Fact]
        public void MilestoneStatus_DeserializesFromJson()
        {
            var json = @"{ ""status"": ""AtRisk"" }";
            var options = new JsonSerializerOptions();
            var doc = JsonDocument.Parse(json);
            var statusStr = doc.RootElement.GetProperty("status").GetString();
            Assert.True(System.Enum.TryParse<MilestoneStatus>(statusStr, out var status));
            Assert.Equal(MilestoneStatus.AtRisk, status);
        }

        [Fact]
        public void WorkItemStatus_DeserializesFromJson()
        {
            var json = @"{ ""status"": ""CarriedOver"" }";
            var doc = JsonDocument.Parse(json);
            var statusStr = doc.RootElement.GetProperty("status").GetString();
            Assert.True(System.Enum.TryParse<WorkItemStatus>(statusStr, out var status));
            Assert.Equal(WorkItemStatus.CarriedOver, status);
        }
    }
}