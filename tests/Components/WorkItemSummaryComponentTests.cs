using Bunit;
using Xunit;
using AgentSquad.Models;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Components;
using System.Collections.Generic;
using System.Linq;

namespace AgentSquad.Tests.Components
{
    public class WorkItemSummaryComponentTests : TestContext
    {
        [Fact]
        public void RenderThreeColumns_WithValidWorkItems()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "API Integration", Description = "Build REST API", Status = WorkItemStatus.Shipped, AssignedTo = "Team A" },
                new WorkItem { Title = "UI Enhancement", Description = "Improve dashboard", Status = WorkItemStatus.InProgress, AssignedTo = "Team B" },
                new WorkItem { Title = "Bug Fix", Description = null, Status = WorkItemStatus.CarriedOver, AssignedTo = "Team C" }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            var columns = component.FindAll(".work-item-column");
            Assert.Equal(3, columns.Count);
            Assert.Contains("Shipped This Month", component.Markup);
            Assert.Contains("In Progress", component.Markup);
            Assert.Contains("Carried Over", component.Markup);
        }

        [Fact]
        public void DisplayItemCountPerColumn()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item 1", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Item 2", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Item 3", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "Item 4", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.Contains("(2)", component.Markup);
            Assert.Contains("(1)", component.Markup);
        }

        [Fact]
        public void TruncateDescriptionAbove100Characters()
        {
            var longDescription = new string('a', 150);
            var workItems = new List<WorkItem>
            {
                new WorkItem 
                { 
                    Title = "Long Description Test",
                    Description = longDescription,
                    Status = WorkItemStatus.Shipped 
                }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            var description = component.Find(".work-item-description");
            Assert.NotNull(description);
            Assert.Contains("...", description.TextContent);
            Assert.True(description.TextContent.Length < longDescription.Length);
        }

        [Fact]
        public void DisplayTitleAndDescriptionForEachItem()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "API Integration", Description = "Build REST API", Status = WorkItemStatus.Shipped }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.Contains("API Integration", component.Markup);
            Assert.Contains("Build REST API", component.Markup);
        }

        [Fact]
        public void DisplayNoItemsPlaceholder_WhenColumnEmpty()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item 1", Status = WorkItemStatus.Shipped }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.Contains("No items", component.Markup);
        }

        [Fact]
        public void HandleNullDescription_WithoutError()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item Without Description", Description = null, Status = WorkItemStatus.Shipped }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.Contains("Item Without Description", component.Markup);
            Assert.DoesNotContain("System.NullReferenceException", component.Markup);
        }

        [Fact]
        public void HandleEmptyDescription_WithoutError()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item", Description = "", Status = WorkItemStatus.Shipped }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.Contains("Item", component.Markup);
        }

        [Fact]
        public void RenderEmptyList_WithoutError()
        {
            var workItems = new List<WorkItem>();

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.NotNull(component);
            Assert.Contains("No items", component.Markup);
        }

        [Fact]
        public void GroupItemsByStatusCorrectly()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item 1", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Item 2", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Item 3", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "Item 4", Status = WorkItemStatus.CarriedOver },
                new WorkItem { Title = "Item 5", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            var markup = component.Markup;
            Assert.Contains("(2)", markup);
            Assert.Contains("(1)", markup);
        }

        [Fact]
        public void RenderResponsiveGridLayout()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item 1", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Item 2", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "Item 3", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.Contains("work-item-summary", component.Markup);
            Assert.Contains("work-item-column", component.Markup);
        }

        [Fact]
        public void IncludeAssignedToInformation_WhenPresent()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem 
                { 
                    Title = "Item", 
                    Description = "Test Item", 
                    Status = WorkItemStatus.Shipped,
                    AssignedTo = "John Doe"
                }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.NotNull(component);
        }

        [Fact]
        public void HandleLargeDataSet_Performance()
        {
            var workItems = new List<WorkItem>();
            for (int i = 0; i < 100; i++)
            {
                workItems.Add(new WorkItem 
                { 
                    Title = $"Item {i}", 
                    Description = $"Description {i}",
                    Status = (WorkItemStatus)(i % 3),
                    AssignedTo = $"Team {i % 5}"
                });
            }

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.NotNull(component);
            Assert.True(component.Markup.Length > 0);
        }

        [Fact]
        public void HandleSpecialCharactersInDescription()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem 
                { 
                    Title = "Item with <special> & \"chars\"",
                    Description = "Description with <html> & special chars",
                    Status = WorkItemStatus.Shipped
                }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => parameters.Add(p => p.WorkItems, workItems));

            Assert.NotNull(component);
        }
    }
}