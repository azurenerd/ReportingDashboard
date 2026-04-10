window.initializeStatusChart = function(canvasId, chartData, chartOptions) {
    const ctx = document.getElementById(canvasId);
    
    if (!ctx) {
        console.error('Canvas element not found: ' + canvasId);
        return;
    }

    // Destroy existing chart if it exists
    if (window.statusChartInstance) {
        window.statusChartInstance.destroy();
    }

    // Create new chart
    window.statusChartInstance = new Chart(ctx, {
        type: 'bar',
        data: chartData,
        options: chartOptions
    });
};