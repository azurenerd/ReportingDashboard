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

window.updateStatusChart = function(canvasId, newData) {
    if (!window.statusChartInstance) {
        console.error('Chart instance not found for ' + canvasId);
        return;
    }

    // Update chart data without full re-initialization
    window.statusChartInstance.data.datasets[0].data = newData;
    
    // Re-render chart smoothly
    window.statusChartInstance.update('none');
};