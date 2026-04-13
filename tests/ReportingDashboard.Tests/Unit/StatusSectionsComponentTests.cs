using Bunit;
using ReportingDashboard.Components.Sections;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class StatusSectionsComponentTests : TestContext
{
    private static List<WorkItem> CreateItems(params (string title, string owner, string notes)[] specs)
    {
        return specs.Select(s => new WorkItem
        {
            Title = s.title,
            Owner = s.owner,
            Notes = s.notes
        }).ToList();
    }

    [Fact]
    public void RendersThreeColumns_WithCorrectHeaderTitlesAndBadgeCounts()
    {
        var shipped = CreateItems(("Item A", "Alice", ""), ("Item B", "Bob", ""));
        var inProgress = CreateItems(("Item C", "Carol", ""));
        var carriedOver = new List<WorkItem>();

        var cut = RenderComponent<StatusSections>(p => p
            .Add(x => x.ShippedItems, shipped)
            .Add(x => x.InProgressItems, inProgress)
            .Add(x => x.CarriedOverItems, carriedOver));

        var columns = cut.FindAll(".status-column");
        Assert.Equal(3, columns.Count);

        var titles = cut.FindAll(".status-header__title");
        Assert.Equal("Shipped", titles[0].TextContent);
        Assert.Equal("In Progress", titles[1].TextContent);
        Assert.Equal("Carried Over", titles[2].TextContent);

        var badges = cut.FindAll(".status-header__badge");
        Assert.Equal("2", badges[0].TextContent);
        Assert.Equal("1", badges[1].TextContent);
        Assert.Equal("0", badges[2].TextContent);
    }

    [Fact]
    public void EmptyCategory_RendersEmptyCardWithMessage()
    {
        var cut = RenderComponent<StatusSections>(p => p
            .Add(x => x.ShippedItems, new List<WorkItem>())
            .Add(x => x.InProgressItems, new List<WorkItem>())
            .Add(x => x.CarriedOverItems, new List<WorkItem>()));

        var emptyCards = cut.FindAll(".status-card--empty");
        Assert.Equal(3, emptyCards.Count);

        var emptyTexts = cut.FindAll(".status-card__empty-text");
        Assert.Equal("No shipped items", emptyTexts[0].TextContent);
        Assert.Equal("No in progress items", emptyTexts[1].TextContent);
        Assert.Equal("No carried over items", emptyTexts[2].TextContent);
    }

    [Fact]
    public void CardRendersTitle_Owner_AndConditionalNotes()
    {
        var shipped = new List<WorkItem>
        {
            new() { Title = "Feature X", Owner = "Alice", Notes = "Shipped early" },
            new() { Title = "Feature Y", Owner = "Bob", Notes = "" }
        };

        var cut = RenderComponent<StatusSections>(p => p
            .Add(x => x.ShippedItems, shipped)
            .Add(x => x.InProgressItems, new List<WorkItem>())
            .Add(x => x.CarriedOverItems, new List<WorkItem>()));

        var cardTitles = cut.FindAll(".status-card__title");
        Assert.Equal("Feature X", cardTitles[0].TextContent);
        Assert.Equal("Feature Y", cardTitles[1].TextContent);

        var owners = cut.FindAll(".status-card__owner");
        Assert.Equal("Alice", owners[0].TextContent);
        Assert.Equal("Bob", owners[1].TextContent);

        // Only "Shipped early" should render; empty string should not
        var notes = cut.FindAll(".status-card__notes");
        Assert.Single(notes);
        Assert.Equal("Shipped early", notes[0].TextContent);
    }

    [Fact]
    public void OwnerIsHidden_WhenNullOrEmpty()
    {
        var items = new List<WorkItem>
        {
            new() { Title = "No owner", Owner = "", Notes = "" }
        };

        var cut = RenderComponent<StatusSections>(p => p
            .Add(x => x.ShippedItems, items)
            .Add(x => x.InProgressItems, new List<WorkItem>())
            .Add(x => x.CarriedOverItems, new List<WorkItem>()));

        var owners = cut.FindAll(".status-card__owner");
        Assert.Empty(owners);
    }

    [Fact]
    public void StatusIndicatorDots_UseCssModifierPerColumn()
    {
        var shipped = CreateItems(("A", "O1", ""));
        var inProgress = CreateItems(("B", "O2", ""));
        var carriedOver = CreateItems(("C", "O3", ""));

        var cut = RenderComponent<StatusSections>(p => p
            .Add(x => x.ShippedItems, shipped)
            .Add(x => x.InProgressItems, inProgress)
            .Add(x => x.CarriedOverItems, carriedOver));

        var indicators = cut.FindAll(".status-indicator");
        Assert.Equal(3, indicators.Count);

        Assert.Contains("status-indicator--shipped", indicators[0].ClassName);
        Assert.Contains("status-indicator--in-progress", indicators[1].ClassName);
        Assert.Contains("status-indicator--carried-over", indicators[2].ClassName);
    }
}