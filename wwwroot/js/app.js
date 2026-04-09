window.addEventListener('beforeunload', function() {
    console.log('Dashboard unloading');
});

document.addEventListener('DOMContentLoaded', function() {
    console.log('Dashboard loaded');
});

window.ChartInterop = {
    createChart: function(canvasId, config) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error('Canvas element not found: ' + canvasId);
            return null;
        }
        
        if (typeof Chart === 'undefined') {
            console.error('Chart.js library not loaded');
            return null;
        }

        return new Chart(ctx, config);
    },

    destroyChart: function(chartInstance) {
        if (chartInstance) {
            chartInstance.destroy();
        }
    }
};