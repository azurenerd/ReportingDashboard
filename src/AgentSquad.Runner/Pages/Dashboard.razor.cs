using Microsoft.AspNetCore.Components;
using AgentSquad.Core.Models;
using AgentSquad.Core.Services;

namespace AgentSquad.Runner.Pages;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject]
    public DashboardDataService DataService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await Task.CompletedTask;
    }

    private void OnDataChanged()
    {
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        await ValueTask.CompletedTask;
    }
}