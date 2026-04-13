using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Components.Sections;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests;

public class StatusSectionsTests : TestContext
{
    [Fact]
    public void Renders_ThreeColumns_WithCorrectCategoriesAndCounts()
    {
        // Arrange
        var workItems = CreateSampleWorkItems();

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert - three columns rendered
        var columns = cut.FindAll(".status-column");
        Assert.Equal(3, columns.Count);

        // Assert - correct header labels in order
        var headers = cut.FindAll(".status-header-title");
        Assert.Equal("SHIPPED", headers[0].TextContent.Trim().ToUpperInvariant());
        Assert.Contains("IN PROGRESS", headers[1].TextContent.Trim().ToUpperInvariant());
        Assert.Contains("CARRIED OVER", headers[2].TextContent.Trim().ToUpperInvariant());

        // Assert - correct badge counts
        var badges = cut.FindAll(".status-header-badge");
        Assert.Equal("4", badges[0].TextContent.Trim());
        Assert.Equal("3", badges[1].TextContent.Trim());
        Assert.Equal("2", badges[2].TextContent.Trim());
    }

    [Fact]
    public void Renders_CorrectItemTitles_InEachColumn()
    {
        // Arrange
        var workItems = CreateSampleWorkItems();

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert - check specific titles appear
        var titles = cut.FindAll(".status-card-title");
        var titleTexts = titles.Select(t => t.TextContent.Trim()).ToList();

        Assert.Contains("Identity service migration", titleTexts);
        Assert.Contains("API gateway configuration", titleTexts);
        Assert.Contains("Legacy data migration", titleTexts);
    }

    [Fact]
    public void EmptyCategory_Renders_NoItemsMessage()
    {
        // Arrange - only Shipped items, no InProgress or CarriedOver
        var workItems = new List<WorkItem>
        {
            new() { Id = "w1", Title = "Done item", Category = "Shipped", Owner = "Alice" }
        };

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert - In Progress and Carried Over show "No items"
        var emptyCards = cut.FindAll(".status-card--empty");
        Assert.Equal(2, emptyCards.Count);
        Assert.All(emptyCards, card =>
            Assert.Contains("No items", card.TextContent));

        // Assert - badges show 0
        var badges = cut.FindAll(".status-header-badge");
        Assert.Equal("1", badges[0].TextContent.Trim()); // Shipped
        Assert.Equal("0", badges[1].TextContent.Trim()); // In Progress
        Assert.Equal("0", badges[2].TextContent.Trim()); // Carried Over
    }

    [Fact]
    public void ConditionalNotes_RenderedOnly_WhenNotEmpty()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new() { Id = "w1", Title = "No notes item", Category = "Shipped", Owner = "Alice", Notes = null },
            new() { Id = "w2", Title = "Has notes item", Category = "Shipped", Owner = "Bob", Notes = "Delayed by vendor" }
        };

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert - only one notes element rendered
        var notes = cut.FindAll(".status-card-notes");
        Assert.Single(notes);
        Assert.Equal("Delayed by vendor", notes[0].TextContent.Trim());
    }

    [Fact]
    public void EmptyWorkItemsList_Renders_AllColumnsWithNoItems()
    {
        // Arrange
        var workItems = new List<WorkItem>();

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert - all three columns rendered
        var columns = cut.FindAll(".status-column");
        Assert.Equal(3, columns.Count);

        // Assert - all badges show 0
        var badges = cut.FindAll(".status-header-badge");
        Assert.All(badges, badge => Assert.Equal("0", badge.TextContent.Trim()));

        // Assert - all columns show empty state
        var emptyCards = cut.FindAll(".status-card--empty");
        Assert.Equal(3, emptyCards.Count);
    }

    [Fact]
    public void HeaderColors_MatchSpecification()
    {
        // Arrange
        var workItems = new List<WorkItem>();

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert - verify inline style colors
        var headers = cut.FindAll(".status-header");
        Assert.Contains("background-color: #28a745", headers[0].GetAttribute("style"));
        Assert.Contains("background-color: #007bff", headers[1].GetAttribute("style"));
        Assert.Contains("background-color: #fd7e14", headers[2].GetAttribute("style"));
    }

    [Fact]
    public void StatusDots_MatchColumnColor()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new() { Id = "w1", Title = "Shipped item", Category = "Shipped", Owner = "Alice" },
            new() { Id = "w2", Title = "In progress item", Category = "InProgress", Owner = "Bob" },
            new() { Id = "w3", Title = "Carried over item", Category = "CarriedOver", Owner = "Carol" }
        };

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert
        var dots = cut.FindAll(".status-dot");
        Assert.Equal(3, dots.Count);
        Assert.Contains("#28a745", dots[0].GetAttribute("style"));
        Assert.Contains("#007bff", dots[1].GetAttribute("style"));
        Assert.Contains("#fd7e14", dots[2].GetAttribute("style"));
    }

    [Fact]
    public void OwnerText_RenderedForEachCard()
    {
        // Arrange
        var workItems = new List<WorkItem>
        {
            new() { Id = "w1", Title = "Item 1", Category = "Shipped", Owner = "Alex Chen" },
            new() { Id = "w2", Title = "Item 2", Category = "InProgress", Owner = "Priya Sharma" }
        };

        // Act
        var cut = RenderComponent<StatusSections>(parameters =>
            parameters.Add(p => p.WorkItems, workItems));

        // Assert
        var owners = cut.FindAll(".status-card-owner");
        var ownerTexts = owners.Select(o => o.TextContent.Trim()).ToList();
        Assert.Contains("Alex Chen", ownerTexts);
        Assert.Contains("Priya Sharma", ownerTexts);
    }

    private static List<WorkItem> CreateSampleWorkItems() => new()
    {
        new() { Id = "w1", Title = "Identity service migration", Category = "Shipped", Owner = "Alex Chen", Notes = null },
        new() { Id = "w2", Title = "Data pipeline v2 rollout", Category = "Shipped", Owner = "Priya Sharma", Notes = null },
        new() { Id = "w3", Title = "Dashboard UI redesign", Category = "Shipped", Owner = "Marcus Johnson", Notes = null },
        new() { Id = "w4", Title = "Monitoring & alerting setup", Category = "Shipped", Owner = "Sara Kim", Notes = null },
        new() { Id = "w5", Title = "API gateway configuration", Category = "InProgress", Owner = "Alex Chen", Notes = "75% complete" },
        new() { Id = "w6", Title = "Partner SDK development", Category = "InProgress", Owner = "Jordan Lee", Notes = "Blocked on API spec" },
        new() { Id = "w7", Title = "Load testing framework", Category = "InProgress", Owner = "Priya Sharma", Notes = null },
        new() { Id = "w8", Title = "Legacy data migration", Category = "CarriedOver", Owner = "Marcus Johnson", Notes = "DBA dependency" },
        new() { Id = "w9", Title = "Compliance audit remediation", Category = "CarriedOver", Owner = "Sara Kim", Notes = "Rescheduled to Q2" }
    };
}