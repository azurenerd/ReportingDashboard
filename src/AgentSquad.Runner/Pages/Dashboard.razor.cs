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
            await base.OnInitializedAsync();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }
}