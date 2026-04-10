let statusChartInstance = null;

function initializeStatusChart(chartData, options) {
    const ctx = document.getElementById('statusChart');
    
    if (!ctx) {
        console.error('Chart canvas element not found');
        return;
    }

    if (statusChartInstance) {
        statusChartInstance.destroy();
    }

    try {
        statusChartInstance = new Chart(ctx, {
            type: 'bar',
            data: chartData,
            options: options
        });
    } catch (error) {
        console.error('Error initializing status chart:', error);
    }
}

function updateStatusChart(newData, maxValue) {
    if (!statusChartInstance) {
        console.error('Chart not initialized');
        return;
    }

    try {
        statusChartInstance.data.datasets[0].data = newData;
        statusChartInstance.options.scales.y.max = maxValue;
        statusChartInstance.update();
    } catch (error) {
        console.error('Error updating status chart:', error);
    }
}