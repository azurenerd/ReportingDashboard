using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class WorkItemSummaryTests : TestContext
{
    [Fact]
    public void Renders_Three_Status_Columns()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "Item 1", Description = "Desc 1", Status = WorkItemStatus.Shipped, AssignedTo = "Team A" },
            new WorkItem { Title = "Item 2", Description = "Desc 2", Status = WorkItemStatus.InProgress, AssignedTo = "Team B" },
            new WorkItem { Title = "Item 3", Description = "Desc 3", Status = WorkItemStatus.CarriedOver, AssignedTo = "Team C" }
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        var columns = component.FindAll(".work-item-column");
        Assert.Equal(3, columns.Count);
        
        Assert.Contains("Shipped This Month", component.Markup);
        Assert.Contains("In Progress", component.Markup);
        Assert.Contains("Carried Over", component.Markup);
    }

    [Fact]
    public void Displays_Correct_Item_Count_Badges()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "S1", Status = WorkItemStatus.Shipped },
            new WorkItem { Title = "S2", Status = WorkItemStatus.Shipped },
            new WorkItem { Title = "S3", Status = WorkItemStatus.Shipped },
            new WorkItem { Title = "I1", Status = WorkItemStatus.InProgress },
            new WorkItem { Title = "I2", Status = WorkItemStatus.InProgress },
            new WorkItem { Title = "C1", Status = WorkItemStatus.CarriedOver }
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        var badges = component.FindAll(".item-count-badge");
        Assert.Equal(3, badges.Count);
        Assert.Contains("3", badges[0].TextContent); // Shipped
        Assert.Contains("2", badges[1].TextContent); // In Progress
        Assert.Contains("1", badges[2].TextContent); // Carried Over
    }

    [Fact]
    public void Truncates_Long_Descriptions_At_150_Characters()
    {
        // Arrange
        var longDesc = new string('a', 200);
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "Test", Description = longDesc, Status = WorkItemStatus.Shipped }
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        var description = component.Find(".item-description");
        Assert.NotNull(description);
        Assert.Contains("...", description.TextContent);
        Assert.True(description.TextContent.Length <= 155); // 150 + "..."
    }

    [Fact]
    public void Handles_Null_WorkItems_List_Gracefully()
    {
        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, (List<WorkItem>)null!)
        );

        // Assert
        Assert.Contains("No work items available", component.Markup);
    }

    [Fact]
    public void Handles_Empty_WorkItems_List_Gracefully()
    {
        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, new List<WorkItem>())
        );

        // Assert
        Assert.Contains("No work items available", component.Markup);
    }

    [Fact]
    public void Hides_Description_When_Null_Or_Empty()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "No Desc", Description = null, Status = WorkItemStatus.Shipped }
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        var items = component.FindAll(".work-item");
        Assert.Single(items);
        Assert.DoesNotContain("item-description", items[0].OuterHtml);
    }

    [Fact]
    public void Hides_AssignedTo_When_Null_Or_Empty()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "No Team", Description = "Test", Status = WorkItemStatus.Shipped, AssignedTo = null }
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        var items = component.FindAll(".work-item");
        Assert.DoesNotContain("item-assigned", items[0].OuterHtml);
    }

    [Fact]
    public void Displays_AssignedTo_Field_When_Present()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "Task", Description = "Desc", Status = WorkItemStatus.Shipped, AssignedTo = "Team A" }
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        Assert.Contains("Team A", component.Markup);
        Assert.Contains("item-assigned", component.Markup);
    }

    [Fact]
    public void Applies_Empty_Column_Styling_When_No_Items()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "S1", Status = WorkItemStatus.Shipped }
            // No InProgress or CarriedOver items
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        var emptyColumns = component.FindAll(".work-item-column.empty");
        Assert.Equal(2, emptyColumns.Count); // InProgress and CarriedOver should be empty
    }

    [Fact]
    public void Renders_All_Items_In_Correct_Columns()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new WorkItem { Title = "Shipped1", Status = WorkItemStatus.Shipped },
            new WorkItem { Title = "Shipped2", Status = WorkItemStatus.Shipped },
            new WorkItem { Title = "InProgress1", Status = WorkItemStatus.InProgress },
            new WorkItem { Title = "CarriedOver1", Status = WorkItemStatus.CarriedOver }
        };

        // Act
        var component = RenderComponent<WorkItemSummary>(parameters =>
            parameters.Add(p => p.WorkItems, workItems)
        );

        // Assert
        Assert.Contains("Shipped1", component.Markup);
        Assert.Contains("Shipped2", component.Markup);
        Assert.Contains("InProgress1", component.Markup);
        Assert.Contains("CarriedOver1", component.Markup);
    }
}