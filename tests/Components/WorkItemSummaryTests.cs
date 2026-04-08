using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class WorkItemSummaryTests : TestContext
    {
        [Fact]
        public void Renders_EmptyState_WhenNoWorkItems()
        {
            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, new List<WorkItem>())
            );

            Assert.Contains("No work items available", component.Markup);
            Assert.Contains("workitems-empty", component.Markup);
        }

        [Fact]
        public void Renders_EmptyState_WhenWorkItemsNull()
        {
            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, (List<WorkItem>)null)
            );

            Assert.Contains("No work items available", component.Markup);
        }

        [Fact]
        public void Renders_ThreeStatusColumns()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Feature A", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "2", Title = "Feature B", Status = WorkItemStatus.InProgress },
                new() { Id = "3", Title = "Feature C", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("Shipped This Month", component.Markup);
            Assert.Contains("In Progress", component.Markup);
            Assert.Contains("Carried Over", component.Markup);
        }

        [Fact]
        public void Displays_ItemCountPerColumn()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Task 1", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "2", Title = "Task 2", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "3", Title = "Task 3", Status = WorkItemStatus.InProgress }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("item-count", component.Markup);
        }

        [Fact]
        public void Renders_SingleWorkItem()
        {
            var workItems = new List<WorkItem>
            {
                new() 
                { 
                    Id = "1", 
                    Title = "Implement Dashboard", 
                    Description = "Build main dashboard layout",
                    Status = WorkItemStatus.InProgress 
                }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("Implement Dashboard", component.Markup);
            Assert.Contains("Build main dashboard layout", component.Markup);
        }

        [Fact]
        public void Renders_MultipleWorkItems_InDifferentStatus()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Released Feature", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "2", Title = "Active Feature", Status = WorkItemStatus.InProgress },
                new() { Id = "3", Title = "Delayed Feature", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("Released Feature", component.Markup);
            Assert.Contains("Active Feature", component.Markup);
            Assert.Contains("Delayed Feature", component.Markup);
        }

        [Fact]
        public void Omits_Description_WhenNull()
        {
            var workItems = new List<WorkItem>
            {
                new() 
                { 
                    Id = "1", 
                    Title = "Task", 
                    Description = null,
                    Status = WorkItemStatus.InProgress 
                }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("Task", component.Markup);
        }

        [Fact]
        public void Includes_Description_WhenProvided()
        {
            var workItems = new List<WorkItem>
            {
                new() 
                { 
                    Id = "1", 
                    Title = "API Implementation", 
                    Description = "Implement REST API endpoints",
                    Status = WorkItemStatus.InProgress 
                }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("Implement REST API endpoints", component.Markup);
        }

        [Fact]
        public void GroupsItems_ByStatus_ShippedThisMonth()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Feature 1", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "2", Title = "Feature 2", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "3", Title = "Feature 3", Status = WorkItemStatus.InProgress }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            var markup = component.Markup;
            var shippedIndex = markup.IndexOf("Shipped This Month");
            var feature1Index = markup.IndexOf("Feature 1");
            var feature2Index = markup.IndexOf("Feature 2");

            Assert.True(shippedIndex < feature1Index);
            Assert.True(feature1Index < feature2Index);
        }

        [Fact]
        public void CountDisplays_Correct_PluralForm()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Task", Status = WorkItemStatus.InProgress }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("1 item", component.Markup);
        }

        [Fact]
        public void CountDisplays_Correct_SingularForm()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Task 1", Status = WorkItemStatus.InProgress },
                new() { Id = "2", Title = "Task 2", Status = WorkItemStatus.InProgress }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("2 items", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_DisplaysThreeStatusColumns()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Done", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "2", Title = "Active", Status = WorkItemStatus.InProgress },
                new() { Id = "3", Title = "Blocked", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("workitems-columns", component.Markup);
            Assert.Contains("Shipped This Month", component.Markup);
            Assert.Contains("In Progress", component.Markup);
            Assert.Contains("Carried Over", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_DisplaysItemCountPerCategory()
        {
            var workItems = new List<WorkItem>
            {
                new() { Id = "1", Title = "Item 1", Status = WorkItemStatus.ShippedThisMonth },
                new() { Id = "2", Title = "Item 2", Status = WorkItemStatus.InProgress }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("item-count", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_IncludesDescriptionForEachItem()
        {
            var workItems = new List<WorkItem>
            {
                new() 
                { 
                    Id = "1", 
                    Title = "Feature X", 
                    Description = "Detailed description",
                    Status = WorkItemStatus.InProgress 
                }
            };

            var component = RenderComponent<WorkItemSummary>(parameters => 
                parameters.Add(p => p.WorkItems, workItems)
            );

            Assert.Contains("Detailed description", component.Markup);
        }
    }
}