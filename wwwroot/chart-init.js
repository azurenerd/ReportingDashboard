window.initMilestoneChart = function(config) {
    if (!config || !config.milestones || config.milestones.length === 0) {
        console.warn('initMilestoneChart: No milestones provided in config');
        return;
    }

    if (!config.containerId) {
        console.error('initMilestoneChart: containerId not provided in config');
        return;
    }

    // Destroy existing chart instance to prevent memory leaks
    if (window.chartInstance && typeof window.chartInstance.destroy === 'function') {
        window.chartInstance.destroy();
        window.chartInstance = null;
    }

    // Map milestone status to color
    const getStatusColor = (status) => {
        const colorMap = {
            'on-track': config.colorScheme?.onTrack || '#28a745',
            'at-risk': config.colorScheme?.atRisk || '#ffc107',
            'delayed': config.colorScheme?.delayed || '#dc3545',
            'completed': config.colorScheme?.completed || '#6c757d'
        };
        return colorMap[status] || '#6c757d';
    };

    // Extract and map milestone data to Chart.js datasets
    const datasets = [{
        label: 'Progress (%)',
        data: config.milestones.map(m => m.progress || 0),
        backgroundColor: config.milestones.map(m => getStatusColor(m.status)),
        borderColor: 'rgba(0, 0, 0, 0.1)',
        borderWidth: 1,
        borderRadius: 4
    }];

    // Create chart data object
    const chartData = {
        labels: config.milestones.map(m => m.name || 'Untitled'),
        datasets: datasets
    };

    // Chart.js configuration for horizontal bar chart
    const chartConfig = {
        type: 'bar',
        data: chartData,
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 8,
                    titleFont: {
                        size: 12
                    },
                    bodyFont: {
                        size: 11
                    },
                    callbacks: {
                        label: function(context) {
                            const milestone = config.milestones[context.dataIndex];
                            const status = milestone.status || 'unknown';
                            const targetDate = milestone.targetDate || 'N/A';
                            return [
                                `Progress: ${context.parsed.x}%`,
                                `Status: ${status}`,
                                `Target: ${targetDate}`
                            ];
                        }
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    max: 100,
                    ticks: {
                        callback: function(value) {
                            return value + '%';
                        }
                    },
                    grid: {
                        display: true,
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    };

    try {
        // Get canvas context and initialize Chart.js
        const canvas = document.getElementById(config.containerId);
        if (!canvas) {
            console.error(`initMilestoneChart: Canvas element with id "${config.containerId}" not found`);
            return;
        }

        const ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error('initMilestoneChart: Failed to get canvas 2D context');
            return;
        }

        // Create and store chart instance globally to prevent memory leaks
        window.chartInstance = new Chart(ctx, chartConfig);
        console.info(`Chart initialized successfully with ${config.milestones.length} milestones`);
    } catch (error) {
        console.error(`initMilestoneChart: Failed to initialize Chart.js: ${error.message}`);
    }
};