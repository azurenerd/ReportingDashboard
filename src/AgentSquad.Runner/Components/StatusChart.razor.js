export function initializeChart(canvas, labels, data) {
    const ctx = canvas.getContext('2d');
    
    const chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Work Items',
                data: data,
                backgroundColor: [
                    '#4CAF50',
                    '#2196F3',
                    '#FF9800'
                ],
                borderColor: [
                    '#388E3C',
                    '#1976D2',
                    '#F57C00'
                ],
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            indexAxis: 'x',
            plugins: {
                legend: {
                    display: true,
                    labels: {
                        font: {
                            family: "'Segoe UI', 'Helvetica Neue', sans-serif",
                            size: 13
                        },
                        color: '#333',
                        padding: 16
                    }
                },
                title: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        font: {
                            family: "'Segoe UI', 'Helvetica Neue', sans-serif",
                            size: 12
                        },
                        color: '#666',
                        stepSize: 1
                    },
                    grid: {
                        color: '#f0f0f0',
                        drawBorder: true
                    },
                    title: {
                        display: false
                    }
                },
                x: {
                    ticks: {
                        font: {
                            family: "'Segoe UI', 'Helvetica Neue', sans-serif",
                            size: 13
                        },
                        color: '#333'
                    },
                    grid: {
                        display: false,
                        drawBorder: true
                    }
                }
            }
        }
    });
    
    return chartInstance;
}

export function updateChart(chartInstance, data) {
    if (!chartInstance || !chartInstance.data) {
        console.warn('Chart instance not available for update');
        return;
    }
    
    chartInstance.data.datasets[0].data = data;
    chartInstance.update('none');
}

export function destroyChart(chartInstance) {
    if (chartInstance && typeof chartInstance.destroy === 'function') {
        chartInstance.destroy();
    }
}