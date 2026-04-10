/**
 * Chart.js Helper Module - Executive Dashboard
 * 
 * Manages Chart.js initialization and updates for the StatusChart component.
 * Handles bar chart creation, color coding, and data updates without full re-initialization.
 * 
 * Usage from Blazor:
 *   const chartModule = await JSRuntime.InvokeAsync("import", "./js/chart-helper.js");
 *   const chart = await chartModule.InvokeAsync("initializeChart", canvasRef, { shipped, inProgress, carriedOver });
 */

let statusChartInstance = null;

export async function initializeChart(canvasElement, statusCounts) {
  // Verify Chart.js is loaded
  if (typeof Chart === 'undefined') {
    console.error('Chart.js not loaded. Ensure CDN script is included in _Host.cshtml');
    return null;
  }

  const ctx = canvasElement.getContext('2d');
  
  // Destroy previous chart if exists
  if (statusChartInstance) {
    statusChartInstance.destroy();
    statusChartInstance = null;
  }

  // Color palette matching dashboard theme
  const colors = {
    shipped: '#4CAF50',
    inProgress: '#2196F3',
    carriedOver: '#FF9800'
  };

  const hoverColors = {
    shipped: '#45a049',
    inProgress: '#1976D2',
    carriedOver: '#E68900'
  };

  // Create bar chart
  statusChartInstance = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: ['Shipped', 'In Progress', 'Carried Over'],
      datasets: [{
        label: 'Work Items by Status',
        data: [
          statusCounts.shipped || 0,
          statusCounts.inProgress || 0,
          statusCounts.carriedOver || 0
        ],
        backgroundColor: [
          colors.shipped,
          colors.inProgress,
          colors.carriedOver
        ],
        borderColor: [
          colors.shipped,
          colors.inProgress,
          colors.carriedOver
        ],
        borderWidth: 1,
        borderRadius: 4,
        hoverBackgroundColor: [
          hoverColors.shipped,
          hoverColors.inProgress,
          hoverColors.carriedOver
        ]
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleColor: '#fff',
          bodyColor: '#fff',
          borderColor: '#ccc',
          borderWidth: 1,
          padding: 12,
          displayColors: false,
          callbacks: {
            label: function(context) {
              return context.parsed.y + ' items';
            }
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1,
            color: '#666',
            font: {
              size: 12
            }
          },
          grid: {
            color: '#e0e0e0'
          },
          title: {
            display: true,
            text: 'Count',
            color: '#333'
          }
        },
        x: {
          ticks: {
            color: '#333',
            font: {
              size: 12
            }
          },
          grid: {
            display: false
          }
        }
      }
    }
  });

  return statusChartInstance;
}

export function updateChartData(statusCounts) {
  if (statusChartInstance) {
    statusChartInstance.data.datasets[0].data = [
      statusCounts.shipped || 0,
      statusCounts.inProgress || 0,
      statusCounts.carriedOver || 0
    ];
    statusChartInstance.update();
  }
}

export function destroyChart() {
  if (statusChartInstance) {
    statusChartInstance.destroy();
    statusChartInstance = null;
  }
}