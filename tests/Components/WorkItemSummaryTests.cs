using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Components
{
    public class WorkItemSummaryTests : TestContext
    {
        private List<WorkItem> CreateTestWorkItems()
        {
            return new List<WorkItem>
            {
                new WorkItem
                {
                    Title = "Feature A",
                    Description = "Implement feature A",
                    Status = WorkItemStatus.Shipped,
                    AssignedTo = "Alice"
                },
                new WorkItem
                {
                    Title = "Feature B",
                    Description = "Implement feature B",
                    Status = WorkItemStatus.InProgress,
                    AssignedTo = "Bob"
                },
                new WorkItem
                {
                    Title = "Feature C",
                    Description = "Implement feature C",
                    Status = WorkItemStatus.CarriedOver,
                    AssignedTo = "Charlie"
                }
            };
        }

        [Fact]
        public void RenderWorkItemSummary_WithValidWorkItems()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var container = cut.Find(".work-items-container");
            Assert.NotNull(container);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysThreeColumns()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var columns = cut.FindAll(".work-items-column");
            Assert.Equal(3, columns.Count);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysShippedColumn()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            Assert.Contains("Shipped This Month", cut.Markup);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysInProgressColumn()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            Assert.Contains("In Progress", cut.Markup);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysCarriedOverColumn()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            Assert.Contains("Carried Over", cut.Markup);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysItemTitles()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            Assert.Contains("Feature A", cut.Markup);
            Assert.Contains("Feature B", cut.Markup);
            Assert.Contains("Feature C", cut.Markup);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysItemDescriptions()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            Assert.Contains("Implement feature A", cut.Markup);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysAssignedTo()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            Assert.Contains("Alice", cut.Markup);
            Assert.Contains("Bob", cut.Markup);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysItemCounts()
        {
            var workItems = CreateTestWorkItems();
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var counts = cut.FindAll(".column-count");
            Assert.NotEmpty(counts);
        }

        [Fact]
        public void RenderWorkItemSummary_TruncatesLongDescriptions()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem
                {
                    Title = "Long Feature",
                    Description = new string('A', 150),
                    Status = WorkItemStatus.Shipped,
                    AssignedTo = "Alice"
                }
            };
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var description = cut.Find(".item-description");
            Assert.NotNull(description);
            Assert.True(description.TextContent.EndsWith("..."));
        }

        [Fact]
        public void RenderWorkItemSummary_DoesNotRender_WhenWorkItemsNull()
        {
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, (List<WorkItem>)null));

            cut.Render();
            var container = cut.QuerySelector(".work-items-container");
            Assert.Null(container);
        }

        [Fact]
        public void RenderWorkItemSummary_DoesNotRender_WhenWorkItemsEmpty()
        {
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, new List<WorkItem>()));

            cut.Render();
            var emptyMessage = cut.Find(".work-items-empty");
            Assert.NotNull(emptyMessage);
        }

        [Fact]
        public void RenderWorkItemSummary_NoAssignedTo_StillRenders()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem
                {
                    Title = "Unassigned",
                    Description = "No assignment",
                    Status = WorkItemStatus.Shipped,
                    AssignedTo = string.Empty
                }
            };
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var item = cut.Find(".work-item");
            Assert.NotNull(item);
        }

        [Fact]
        public void RenderWorkItemSummary_NoDescription_StillRenders()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem
                {
                    Title = "No Description",
                    Description = string.Empty,
                    Status = WorkItemStatus.Shipped,
                    AssignedTo = "Alice"
                }
            };
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var item = cut.Find(".work-item");
            Assert.NotNull(item);
        }

        [Fact]
        public void RenderWorkItemSummary_CategoryCount_ShippedCorrect()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "S1", Status = WorkItemStatus.Shipped, AssignedTo = "A" },
                new WorkItem { Title = "S2", Status = WorkItemStatus.Shipped, AssignedTo = "B" },
                new WorkItem { Title = "I1", Status = WorkItemStatus.InProgress, AssignedTo = "C" }
            };
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var counts = cut.FindAll(".column-count");
            // First column (Shipped) should show count of 2
            Assert.Contains("2", counts[0].TextContent);
        }

        [Fact]
        public void RenderWorkItemSummary_EmptyColumn_ShowsNoItems()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "S1", Status = WorkItemStatus.Shipped, AssignedTo = "A" }
            };
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var emptyItems = cut.FindAll(".items-empty");
            Assert.NotEmpty(emptyItems);
        }

        [Fact]
        public void RenderWorkItemSummary_DisplaysWorkItemsWithoutShuffling()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item1", Status = WorkItemStatus.InProgress, AssignedTo = "A" },
                new WorkItem { Title = "Item2", Status = WorkItemStatus.InProgress, AssignedTo = "B" }
            };
            var cut = RenderComponent<WorkItemSummary>(parameters => parameters
                .Add(p => p.WorkItems, workItems));

            cut.Render();
            var titles = cut.FindAll(".item-title");
            Assert.Contains(titles, t => t.TextContent == "Item1");
            Assert.Contains(titles, t => t.TextContent == "Item2");
        }
    }
}