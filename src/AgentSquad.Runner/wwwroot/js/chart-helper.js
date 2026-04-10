/**
 * Chart.js Helper Module - Executive Dashboard
 * 
 * Purpose: Initialize and manage Chart.js instances for status chart visualization.
 * Handles data updates, color coding, and responsive rendering.
 * 
 * Usage: Import via ES6 module in Blazor component
 *   const chartModule = await JSRuntime.InvokeAsync("import", "./js/chart-helper.js");
 *   await chartModule.InvokeAsync("initializeChart", canvasRef, { shipped, inProgress, carriedOver });
 */

export async function initializeChart(canvasElement, statusCounts) {
  // Ensure Chart.js is loaded (CDN)
  if (typeof Chart === 'undefined') {
    console.error('Chart.js library not loaded. Ensure CDN is included in _Host.cshtml');
    return null;
  }

  const ctx = canvasElement.getContext('2d');
  
  // Destroy existing chart if present
  if (window.statusChartInstance) {
    window.statusChartInstance.destroy();
  }

  // Chart colors matching dashboard theme
  const colors = {
    shipped: '#4CAF50',      // Green
    inProgress: '#2196F3',   // Blue
    carriedOver: '#FF9800'   // Orange
  };

  // Create bar chart
  const chart = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: ['Shipped', 'In Progress', 'Carried Over'],
      datasets: [{
        label: 'Work Items by Status',
        data: [
          statusCounts.shipped,
          statusCounts.inProgress,
          statusCounts.carriedOver
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
          '#45a049',  // Darker green
          '#1976D2',  // Darker blue
          '#E68900'   // Darker orange
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
          displayColors: false
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1,
            color: '#666'
          },
          grid: {
            color: '#e0e0e0',
            drawBorder: true
          }
        },
        x: {
          ticks: {
            color: '#333'
          },
          grid: {
            display: false
          }
        }
      }
    }
  });

  // Cache instance for future updates
  window.statusChartInstance = chart;
  
  return chart;
}

export function updateChart(statusCounts) {
  if (window.statusChartInstance) {
    window.statusChartInstance.data.datasets[0].data = [
      statusCounts.shipped,
      statusCounts.inProgress,
      statusCounts.carriedOver
    ];
    window.statusChartInstance.update();
  }
}

export function destroyChart() {
  if (window.statusChartInstance) {
    window.statusChartInstance.destroy();
    window.statusChartInstance = null;
  }
}