// Chart.js Initialization & Visualization

window.Dashboard = window.Dashboard || {};

window.Dashboard.initializeCharts = function() {
    // Initialize timeline chart if Chart.js available
    if (typeof Chart === 'undefined') {
        console.warn('Chart.js not available - timeline will use HTML rendering');
        return;
    }

    const timelineCanvas = document.getElementById('timelineChart');
    if (!timelineCanvas) {
        return; // No canvas element found
    }

    const ctx = timelineCanvas.getContext('2d');
    
    // Timeline chart configuration
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['M1', 'M2', 'M3', 'M4', 'M5'],
            datasets: [{
                label: 'Milestone Progress',
                data: [100, 100, 70, 40, 0],
                backgroundColor: [
                    '#10b981',
                    '#10b981',
                    '#3b82f6',
                    '#ef4444',
                    '#d1d5db'
                ],
                borderRadius: 4
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: { max: 100 }
            }
        }
    });
};

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.Dashboard.initializeCharts);
} else {
    window.Dashboard.initializeCharts();
}