// Dashboard Application Logic

document.addEventListener('DOMContentLoaded', function() {
    // Initialize dashboard on page load
    initializeDashboard();
});

function initializeDashboard() {
    // Load data from data.json
    fetch('data.json')
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to load dashboard data');
            }
            return response.json();
        })
        .then(data => {
            renderDashboard(data);
        })
        .catch(error => {
            console.error('Dashboard initialization error:', error);
            displayError('Failed to load dashboard data. ' + error.message);
        });
}

function renderDashboard(data) {
    // Render project header
    const headerElement = document.querySelector('.dashboard-header h1');
    if (headerElement) {
        headerElement.textContent = data.project.name;
    }

    // Render metrics
    renderMetrics(data.metrics);

    // Render timeline
    renderTimeline(data.milestones);

    // Render work items
    renderWorkItems(data.workItems);

    // Initialize charts (if Chart.js available)
    if (window.Dashboard && window.Dashboard.initializeCharts) {
        window.Dashboard.initializeCharts();
    }
}

function renderMetrics(metrics) {
    const metricsGrid = document.querySelector('.metrics-grid');
    if (!metricsGrid) return;

    metricsGrid.innerHTML = `
        <div class="metric-card success">
            <div class="metric-card-label">Completion</div>
            <div class="metric-card-value">${metrics.completionPercentage}%</div>
            <div class="metric-card-detail">Overall progress</div>
        </div>
        <div class="metric-card ${metrics.healthStatus === 'ontrack' ? 'success' : 'danger'}">
            <div class="metric-card-label">Health Status</div>
            <div class="metric-card-value">${metrics.healthStatus}</div>
            <div class="metric-card-detail">Project status</div>
        </div>
        <div class="metric-card info">
            <div class="metric-card-label">Velocity</div>
            <div class="metric-card-value">${metrics.velocityThisMonth}</div>
            <div class="metric-card-detail">Items this month</div>
        </div>
        <div class="metric-card info">
            <div class="metric-card-label">Milestones</div>
            <div class="metric-card-value">${metrics.milestonesCompleted}/${metrics.milestonesTotal}</div>
            <div class="metric-card-detail">Completed/Total</div>
        </div>
    `;
}

function renderTimeline(milestones) {
    const timeline = document.querySelector('.timeline');
    if (!timeline) return;

    timeline.innerHTML = milestones.map(m => `
        <div class="timeline-item ${m.status}">
            <div style="font-weight: 600; margin-bottom: 0.5rem;">${m.name}</div>
            <div style="font-size: 0.875rem; color: #374151; margin-bottom: 0.5rem;">${dashboardUtils.formatDate(m.targetDate)}</div>
            <div style="font-size: 0.75rem; text-transform: uppercase; font-weight: 600;">${m.status}</div>
        </div>
    `).join('');
}

function renderWorkItems(items) {
    const grid = document.querySelector('.work-items-grid');
    if (!grid) return;

    grid.innerHTML = `
        <div class="work-item-column">
            <div class="work-item-column-header">
                <div class="work-item-column-title">Shipped This Month</div>
                <div class="work-item-count">${items.shipped.length}</div>
            </div>
            ${items.shipped.map(item => `
                <div class="work-item">
                    <div class="work-item-title">${item.title}</div>
                    <div class="work-item-description">${item.description}</div>
                </div>
            `).join('')}
        </div>
        <div class="work-item-column">
            <div class="work-item-column-header">
                <div class="work-item-column-title">In Progress</div>
                <div class="work-item-count">${items.inProgress.length}</div>
            </div>
            ${items.inProgress.map(item => `
                <div class="work-item">
                    <div class="work-item-title">${item.title}</div>
                    <div class="work-item-description">${item.description}</div>
                </div>
            `).join('')}
        </div>
        <div class="work-item-column">
            <div class="work-item-column-header">
                <div class="work-item-column-title">Carried Over</div>
                <div class="work-item-count">${items.carriedOver.length}</div>
            </div>
            ${items.carriedOver.map(item => `
                <div class="work-item">
                    <div class="work-item-title">${item.title}</div>
                    <div class="work-item-description">${item.description}</div>
                </div>
            `).join('')}
        </div>
    `;
}

function displayError(message) {
    const appDiv = document.getElementById('app');
    if (appDiv) {
        appDiv.innerHTML = `<div class="alert alert-danger m-5"><h4>Error</h4><p>${message}</p></div>`;
    }
}