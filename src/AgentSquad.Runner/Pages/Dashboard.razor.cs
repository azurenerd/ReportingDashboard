using Microsoft.AspNetCore.Components;
using AgentSquad.Core.Models;
using AgentSquad.Core.Services;

namespace AgentSquad.Runner.Pages;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject]
    public DashboardDataService DataService { get; set; } = null!;

    private bool _disposed = false;

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
        if (_disposed)
        {
            return;
        }

        DataService.OnDataChanged -= OnDataChanged;
        _disposed = true;
        await ValueTask.CompletedTask;
    }
}