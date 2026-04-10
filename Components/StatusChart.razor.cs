using Microsoft.AspNetCore.Components;
using ChartJS.Blazor.BarChart;
using ChartJS.Blazor.Common;

namespace AgentSquad.Runner.Components
{
    public partial class StatusChart : ComponentBase
    {
        [Parameter]
        public (int Shipped, int InProgress, int CarriedOver) StatusCounts { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private (int Shipped, int InProgress, int CarriedOver) _previousCounts;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }
    }
}