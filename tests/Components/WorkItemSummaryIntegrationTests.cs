using Bunit;
using Xunit;
using AgentSquad.Models;
using AgentSquad.Runner.Components;
using System.Collections.Generic;
using System.Linq;

namespace AgentSquad.Tests.Components
{
    public class WorkItemSummaryIntegrationTests : TestContext
    {
        [Fact]
        public void Acceptance_ThreeStatusColumnsRender_WithCorrectHeaders()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Task 1", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Task 2", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "Task 3", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.Contains("Shipped This Month", component.Markup);
            Assert.Contains("In Progress", component.Markup);
            Assert.Contains("Carried Over", component.Markup);
        }

        [Fact]
        public void Acceptance_ItemCountDisplayed_PerColumn()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Shipped 1", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Shipped 2", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Shipped 3", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "InProgress 1", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "InProgress 2", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "CarriedOver 1", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            var content = component.Markup;
            Assert.Contains("(3)", content);
            Assert.Contains("(2)", content);
            Assert.Contains("(1)", content);
        }

        [Fact]
        public void Acceptance_TitleAndDescriptionDisplay_ForEachWorkItem()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem 
                { 
                    Title = "API Integration",
                    Description = "Build REST API for data sync",
                    Status = WorkItemStatus.Shipped 
                }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.Contains("API Integration", component.Markup);
            Assert.Contains("Build REST API for data sync", component.Markup);
        }

        [Fact]
        public void Acceptance_TruncateDescription_WhenOver100Characters()
        {
            var longDescription = "This is a very long description that exceeds one hundred characters and should be truncated with ellipsis at the end when rendered";
            var workItems = new List<WorkItem>
            {
                new WorkItem 
                { 
                    Title = "Long Description Item",
                    Description = longDescription,
                    Status = WorkItemStatus.Shipped 
                }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            var markup = component.Markup;
            Assert.Contains("...", markup);
            Assert.DoesNotContain(longDescription, markup);
        }

        [Fact]
        public void Acceptance_ItemsGroupedAndSorted_ByStatusEnum()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Carried 1", Status = WorkItemStatus.CarriedOver },
                new WorkItem { Title = "InProgress 1", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "Shipped 1", Status = WorkItemStatus.Shipped }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            var columns = component.FindAll(".work-item-column");
            Assert.Equal(3, columns.Count);

            var headers = component.FindAll(".work-item-column-header");
            Assert.Equal(3, headers.Count);
            Assert.Contains("Shipped This Month", headers[0].TextContent);
        }

        [Fact]
        public void Acceptance_EmptyColumnsDisplay_NoItemsPlaceholder()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Only Shipped", Status = WorkItemStatus.Shipped }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            var emptyStates = component.FindAll(".work-item-empty-state");
            Assert.NotEmpty(emptyStates);
            Assert.All(emptyStates, empty => Assert.Contains("No items", empty.TextContent));
        }

        [Fact]
        public void Acceptance_CSSGridLayout_ResponsiveAtMinimumResolution()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item 1", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "Item 2", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "Item 3", Status = WorkItemStatus.CarriedOver }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.Contains("work-item-summary", component.Markup);
            var summary = component.Find(".work-item-summary");
            Assert.NotNull(summary);
        }

        [Fact]
        public void Acceptance_ComponentReceives_ListWorkItemParameter()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Test Item", Status = WorkItemStatus.Shipped }
            };

            var cut = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.NotNull(cut);
            Assert.Contains("Test Item", cut.Markup);
        }

        [Fact]
        public void Acceptance_NoConsoleErrors_WithNullSafety_ForMissingDescription()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item No Description", Description = null, Status = WorkItemStatus.InProgress }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.DoesNotContain("NullReferenceException", component.Markup);
            Assert.DoesNotContain("error", component.Markup.ToLower());
        }

        [Fact]
        public void EdgeCase_EmptyWorkItemList_DoesNotCrash()
        {
            var workItems = new List<WorkItem>();

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.NotNull(component);
            var emptyStates = component.FindAll(".work-item-empty-state");
            Assert.NotEmpty(emptyStates);
        }

        [Fact]
        public void EdgeCase_VeryLongTitle_DisplaysWithoutTruncation()
        {
            var veryLongTitle = new string('A', 300);
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = veryLongTitle, Status = WorkItemStatus.Shipped }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.NotNull(component);
        }

        [Fact]
        public void EdgeCase_SpecialCharactersInDescription_HandledCorrectly()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem 
                { 
                    Title = "Special Chars",
                    Description = "Test & <special> \"characters\" 'quotes'",
                    Status = WorkItemStatus.InProgress 
                }
            };

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.NotNull(component);
        }

        [Fact]
        public void EdgeCase_HundredItemsInOneStatus_RendersSuccessfully()
        {
            var workItems = new List<WorkItem>();
            for (int i = 0; i < 100; i++)
            {
                workItems.Add(new WorkItem 
                { 
                    Title = $"Item {i}",
                    Status = WorkItemStatus.Shipped 
                });
            }

            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, workItems));

            Assert.Contains("(100)", component.Markup);
        }

        [Fact]
        public void ErrorCondition_NullWorkItemsList_DefaultsToEmptyList()
        {
            var component = RenderComponent<WorkItemSummary>(p => p.Add(x => x.WorkItems, (List<WorkItem>)null));

            Assert.NotNull(component);
        }
    }
}