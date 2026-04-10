// Chart.js initialization helper for StatusChart component
window.initStatusChart = function(elementId, config) {
    const ctx = document.getElementById(elementId);
    if (!ctx) {
        console.warn(`Canvas element with id '${elementId}' not found`);
        return;
    }

    // Destroy existing chart if present
    if (window.statusChartInstance) {
        window.statusChartInstance.destroy();
    }

    // Create new chart
    if (typeof Chart !== 'undefined') {
        window.statusChartInstance = new Chart(ctx, config);
    } else {
        console.warn('Chart.js library not loaded');
    }
};