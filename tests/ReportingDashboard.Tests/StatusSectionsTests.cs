using Bunit;
using ReportingDashboard.Components.Sections;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests;

public class StatusSectionsTests : TestContext
{
    [Fact]
    public void Renders_Three_Columns_With_Correct_Headers()
    {
        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, new List<WorkItem>())
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        var headers = cut.FindAll(".status-header__title");
        Assert.Equal(3, headers.Count);
        Assert.Equal("Shipped", headers[0].TextContent);
        Assert.Equal("In Progress", headers[1].TextContent);
        Assert.Equal("Carried Over", headers[2].TextContent);
    }

    [Fact]
    public void Renders_Three_Columns_In_Grid()
    {
        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, new List<WorkItem>())
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        var grid = cut.Find(".status-sections");
        Assert.NotNull(grid);

        var columns = cut.FindAll(".status-column");
        Assert.Equal(3, columns.Count);
    }

    [Fact]
    public void Empty_Categories_Show_Zero_Badge_And_Empty_State()
    {
        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, new List<WorkItem>())
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        var badges = cut.FindAll(".status-header__badge");
        Assert.Equal(3, badges.Count);
        Assert.All(badges, badge => Assert.Equal("0", badge.TextContent));

        var emptyCards = cut.FindAll(".status-card--empty");
        Assert.Equal(3, emptyCards.Count);
    }

    [Fact]
    public void Renders_Correct_Item_Counts_In_Badges()
    {
        var shipped = CreateItems(4);
        var inProgress = CreateItems(3);
        var carriedOver = CreateItems(2);

        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, shipped)
            .Add(p => p.InProgressItems, inProgress)
            .Add(p => p.CarriedOverItems, carriedOver));

        var badges = cut.FindAll(".status-header__badge");
        Assert.Equal("4", badges[0].TextContent);
        Assert.Equal("3", badges[1].TextContent);
        Assert.Equal("2", badges[2].TextContent);
    }

    [Fact]
    public void Renders_Card_With_Title_And_Owner()
    {
        var items = new List<WorkItem>
        {
            new() { Title = "Build API", Owner = "Alice", Notes = "" }
        };

        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, items)
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        var title = cut.Find(".status-card__title");
        Assert.Equal("Build API", title.TextContent);

        var owner = cut.Find(".status-card__owner");
        Assert.Equal("Alice", owner.TextContent);
    }

    [Fact]
    public void Renders_Notes_Only_When_Present()
    {
        var withNotes = new List<WorkItem>
        {
            new() { Title = "Item With Notes", Owner = "Bob", Notes = "Important note" }
        };
        var withoutNotes = new List<WorkItem>
        {
            new() { Title = "Item Without Notes", Owner = "Carol", Notes = "" }
        };

        var cutWith = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, withNotes)
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        var notes = cutWith.FindAll(".status-card__notes");
        Assert.Single(notes);
        Assert.Equal("Important note", notes[0].TextContent);

        var cutWithout = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, withoutNotes)
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        var noNotes = cutWithout.FindAll(".status-card__notes");
        Assert.Empty(noNotes);
    }

    [Fact]
    public void Hides_Owner_When_Empty()
    {
        var items = new List<WorkItem>
        {
            new() { Title = "No Owner Item", Owner = "", Notes = "" }
        };

        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, items)
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        var owners = cut.FindAll(".status-card__owner");
        Assert.Empty(owners);
    }

    [Fact]
    public void Renders_Status_Indicator_Dots_With_Correct_Css_Modifiers()
    {
        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, CreateItems(1))
            .Add(p => p.InProgressItems, CreateItems(1))
            .Add(p => p.CarriedOverItems, CreateItems(1)));

        Assert.NotNull(cut.Find(".status-indicator--shipped"));
        Assert.NotNull(cut.Find(".status-indicator--in-progress"));
        Assert.NotNull(cut.Find(".status-indicator--carried-over"));
    }

    [Fact]
    public void Renders_Correct_Header_Color_Classes()
    {
        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, new List<WorkItem>())
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, new List<WorkItem>()));

        Assert.NotNull(cut.Find(".status-header--shipped"));
        Assert.NotNull(cut.Find(".status-header--in-progress"));
        Assert.NotNull(cut.Find(".status-header--carried-over"));
    }

    [Fact]
    public void Mixed_Empty_And_Populated_Categories_Render_Correctly()
    {
        var cut = RenderComponent<StatusSections>(parameters => parameters
            .Add(p => p.ShippedItems, CreateItems(3))
            .Add(p => p.InProgressItems, new List<WorkItem>())
            .Add(p => p.CarriedOverItems, CreateItems(1)));

        var badges = cut.FindAll(".status-header__badge");
        Assert.Equal("3", badges[0].TextContent);
        Assert.Equal("0", badges[1].TextContent);
        Assert.Equal("1", badges[2].TextContent);

        var emptyCards = cut.FindAll(".status-card--empty");
        Assert.Single(emptyCards);
    }

    private static List<WorkItem> CreateItems(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new WorkItem
            {
                Title = $"Work Item {i}",
                Owner = $"Owner {i}",
                Notes = i % 2 == 0 ? $"Note for item {i}" : ""
            })
            .ToList();
    }
}