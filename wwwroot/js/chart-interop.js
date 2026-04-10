export async function initChart(canvasId, milestones) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error(`Canvas element with id '${canvasId}' not found`);
        return;
    }

    const ctx = canvas.getContext('2d');

    const labels = milestones.map(m => m.name);
    const dates = milestones.map(m => new Date(m.targetDate));
    const backgroundColor = milestones.map(m => m.color);

    const chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Milestone Target Dates',
                    data: dates.map((d, i) => ({
                        x: d.toLocaleDateString(),
                        y: i + 1
                    })),
                    backgroundColor: backgroundColor,
                    borderColor: '#333',
                    borderWidth: 1
                }
            ]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const milestone = milestones[context.dataIndex];
                            return `Status: ${milestone.status}`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return milestones[value - 1]?.name || '';
                        }
                    }
                },
                x: {
                    type: 'linear',
                    display: true
                }
            }
        }
    });

    return chart;
}