using Microsoft.AspNetCore.Components;
using ChartJS.Blazor.BarChart;
using ChartJS.Blazor.Common;
using System.Collections.Generic;

namespace AgentSquad.Runner.Components
{
    public partial class StatusChart : ComponentBase
    {
        [Parameter]
        public (int Shipped, int InProgress, int CarriedOver) StatusCounts { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private (int Shipped, int InProgress, int CarriedOver) _previousCounts;
        private object _chartReference;
        private bool _chartInitialized = false;

        protected override async Task OnParametersSetAsync()
        {
            if (_previousCounts != StatusCounts)
            {
                _previousCounts = StatusCounts;
                
                if (!_chartInitialized)
                {
                    await InitializeChartAsync();
                    _chartInitialized = true;
                }
                else
                {
                    await UpdateChartDataAsync();
                }
            }
        }

        private async Task InitializeChartAsync()
        {
            if (JSRuntime == null)
                return;

            var chartData = new
            {
                labels = new[] { "Shipped", "In Progress", "Carried Over" },
                datasets = new[]
                {
                    new
                    {
                        label = "Work Items",
                        data = new[] { StatusCounts.Shipped, StatusCounts.InProgress, StatusCounts.CarriedOver },
                        backgroundColor = new[] { "#4CAF50", "#2196F3", "#FF9800" },
                        borderColor = new[] { "#45a049", "#0b7dda", "#e68900" },
                        borderWidth = 1
                    }
                }
            };

            var chartOptions = new
            {
                responsive = true,
                maintainAspectRatio = true,
                plugins = new
                {
                    legend = new
                    {
                        display = true,
                        position = "top"
                    },
                    title = new
                    {
                        display = false
                    }
                },
                scales = new
                {
                    y = new
                    {
                        beginAtZero = true,
                        title = new
                        {
                            display = true,
                            text = "Count"
                        },
                        ticks = new
                        {
                            stepSize = 1
                        }
                    }
                }
            };

            try
            {
                await JSRuntime.InvokeVoidAsync("window.initializeStatusChart", "statusChart", chartData, chartOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing chart: {ex.Message}");
            }
        }

        private async Task UpdateChartDataAsync()
        {
            if (JSRuntime == null)
                return;

            var newData = new[] { StatusCounts.Shipped, StatusCounts.InProgress, StatusCounts.CarriedOver };

            try
            {
                await JSRuntime.InvokeVoidAsync("window.updateStatusChart", "statusChart", newData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating chart: {ex.Message}");
            }
        }
    }
}