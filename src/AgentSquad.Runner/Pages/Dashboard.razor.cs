using Microsoft.AspNetCore.Components;
using AgentSquad.Core.Models;
using AgentSquad.Core.Services;

namespace AgentSquad.Runner.Pages;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject]
    public DashboardDataService DataService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        DataService.OnDataChanged += OnDataChanged;
        await Task.CompletedTask;
    }

    private void OnDataChanged()
    {
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        DataService.OnDataChanged -= OnDataChanged;
        await ValueTask.CompletedTask;
    }
}