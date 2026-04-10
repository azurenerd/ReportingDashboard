using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Components;

namespace AgentSquad.Runner.Pages
{
    public partial class Dashboard : ComponentBase, IAsyncDisposable
    {
        [Inject]
        public DashboardDataService DataService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (DataService != null)
            {
                DataService.OnDataChanged += OnDataChanged;
            }
            await base.OnInitializedAsync();
        }

        private void OnDataChanged()
        {
            StateHasChanged();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (DataService != null)
            {
                DataService.OnDataChanged -= OnDataChanged;
            }
            await Task.CompletedTask;
        }
    }
}