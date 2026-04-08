// Dashboard initialization and data binding
document.addEventListener('DOMContentLoaded', function() {
    initializeDashboard();
});

function initializeDashboard() {
    console.log('Dashboard initialized');
    // Timeline and chart initialization will be handled by Blazor components
}

// Timeline Chart Configuration
function initializeTimeline(canvasId, data) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.labels,
            datasets: data.datasets
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    display: false
                },
                x: {
                    display: true
                }
            }
        }
    });
}

// Utility function for formatting dates
function formatDate(date) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(date).toLocaleDateString('en-US', options);
}