let chartInstances = new Map();

export async function initializeChart(canvas, labels, data) {
    // Dynamically load Chart.js from CDN if not already loaded
    if (typeof Chart === 'undefined') {
        await loadChartJs();
    }

    const ctx = canvas.getContext('2d');
    
    const chartConfig = {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Work Items',
                    data: data,
                    backgroundColor: [
                        '#4CAF50', // Green for Shipped
                        '#2196F3', // Blue for In Progress
                        '#FF9800'  // Orange for Carried Over
                    ],
                    borderColor: [
                        '#45a049',
                        '#0b7dda',
                        '#e68900'
                    ],
                    borderWidth: 1,
                    borderRadius: 4,
                    barPercentage: 0.7,
                    categoryPercentage: 0.8
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'x',
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        font: {
                            family: "'Segoe UI', 'Helvetica Neue', sans-serif",
                            size: 14
                        },
                        color: '#333',
                        padding: 15,
                        usePointStyle: false
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14
                    },
                    bodyFont: {
                        size: 13
                    },
                    cornerRadius: 4,
                    displayColors: true,
                    borderColor: '#ddd',
                    borderWidth: 1
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    min: 0,
                    ticks: {
                        stepSize: 1,
                        font: {
                            family: "'Segoe UI', 'Helvetica Neue', sans-serif",
                            size: 12
                        },
                        color: '#666',
                        padding: 8
                    },
                    grid: {
                        color: '#e0e0e0',
                        drawBorder: true,
                        drawTicks: true
                    },
                    title: {
                        display: true,
                        text: 'Count',
                        font: {
                            size: 13
                        },
                        color: '#666'
                    }
                },
                x: {
                    ticks: {
                        font: {
                            family: "'Segoe UI', 'Helvetica Neue', sans-serif",
                            size: 13
                        },
                        color: '#333',
                        padding: 8
                    },
                    grid: {
                        color: 'transparent',
                        drawBorder: true,
                        drawTicks: false
                    }
                }
            }
        }
    };

    const chart = new Chart(ctx, chartConfig);
    chartInstances.set(canvas, chart);
    
    return canvas;
}

export function updateChart(canvas, data) {
    const chart = chartInstances.get(canvas);
    
    if (chart) {
        chart.data.datasets[0].data = data;
        chart.update('none');
    }
}

export function destroyChart(canvas) {
    const chart = chartInstances.get(canvas);
    
    if (chart) {
        chart.destroy();
        chartInstances.delete(canvas);
    }
}

function loadChartJs() {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.js';
        script.async = true;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error('Failed to load Chart.js'));
        document.head.appendChild(script);
    });
}