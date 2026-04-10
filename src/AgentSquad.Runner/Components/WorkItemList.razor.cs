using Microsoft.AspNetCore.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Components;

public partial class WorkItemList : ComponentBase
{
    [Parameter]
    public IReadOnlyList<WorkItem> Shipped { get; set; } = new List<WorkItem>().AsReadOnly();

    [Parameter]
    public IReadOnlyList<WorkItem> InProgress { get; set; } = new List<WorkItem>().AsReadOnly();

    [Parameter]
    public IReadOnlyList<WorkItem> CarriedOver { get; set; } = new List<WorkItem>().AsReadOnly();
}