using Microsoft.AspNetCore.Components;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Pages;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject]
    public required IDashboardDataService DataService { get; set; }

    private string? _errorMessage;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        
        if (DataService.GetCurrentData() == null && DataService.GetLastError() != null)
        {
            _errorMessage = DataService.GetLastError();
        }

        DataService.OnDataChanged += OnDataChanged;
        _isLoading = false;
        await Task.CompletedTask;
    }

    private void OnDataChanged()
    {
        _errorMessage = null;
        StateHasChanged();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (DataService != null)
        {
            DataService.OnDataChanged -= OnDataChanged;
        }
        await ValueTask.CompletedTask;
    }
}