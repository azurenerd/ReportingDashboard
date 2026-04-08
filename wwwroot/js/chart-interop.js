window.ChartInterop = {
    initializeBurndownChart: function(canvasElementId, labels, data) {
        try {
            const canvas = document.getElementById(canvasElementId);
            if (!canvas) {
                console.error(`Canvas element with id '${canvasElementId}' not found`);
                return false;
            }

            if (typeof Chart === 'undefined') {
                console.error('Chart.js library is not loaded');
                return false;
            }

            const existingChart = window.__charts = window.__charts || {};
            if (existingChart[canvasElementId]) {
                existingChart[canvasElementId].destroy();
            }

            const ctx = canvas.getContext('2d');
            const chart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Burn-down Trajectory',
                            data: data,
                            borderColor: '#0d6efd',
                            backgroundColor: 'rgba(13, 110, 253, 0.1)',
                            borderWidth: 2,
                            fill: true,
                            pointRadius: 4,
                            pointBackgroundColor: '#0d6efd',
                            pointBorderColor: '#fff',
                            pointBorderWidth: 2,
                            tension: 0.4
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    animation: false,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top',
                            labels: {
                                font: {
                                    size: 12
                                },
                                usePointStyle: true,
                                padding: 15
                            }
                        },
                        tooltip: {
                            enabled: true,
                            mode: 'index',
                            intersect: false,
                            backgroundColor: 'rgba(0,0,0,0.8)',
                            titleFont: {
                                size: 12
                            },
                            bodyFont: {
                                size: 11
                            },
                            padding: 10
                        }
                    },
                    scales: {
                        x: {
                            display: true,
                            title: {
                                display: true,
                                text: 'Date',
                                font: {
                                    size: 12
                                }
                            },
                            grid: {
                                drawBorder: true,
                                color: 'rgba(0,0,0,0.1)'
                            }
                        },
                        y: {
                            display: true,
                            title: {
                                display: true,
                                text: 'Tasks Remaining',
                                font: {
                                    size: 12
                                }
                            },
                            beginAtZero: true,
                            grid: {
                                drawBorder: true,
                                color: 'rgba(0,0,0,0.1)'
                            }
                        }
                    }
                }
            });

            existingChart[canvasElementId] = chart;
            console.log(`Burn-down chart initialized successfully for ${canvasElementId}`);
            return true;
        } catch (error) {
            console.error('Error initializing burn-down chart:', error);
            return false;
        }
    },

    destroyChart: function(canvasElementId) {
        try {
            const existingCharts = window.__charts = window.__charts || {};
            if (existingCharts[canvasElementId]) {
                existingCharts[canvasElementId].destroy();
                delete existingCharts[canvasElementId];
                console.log(`Chart ${canvasElementId} destroyed successfully`);
            }
        } catch (error) {
            console.error('Error destroying chart:', error);
        }
    }
};