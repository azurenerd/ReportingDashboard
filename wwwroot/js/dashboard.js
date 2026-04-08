// Dashboard Data Loading & Rendering

window.Dashboard = window.Dashboard || {};

window.Dashboard.loadData = async function() {
    try {
        const response = await fetch('data.json');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        window.Dashboard.renderDashboard(data);
    } catch (error) {
        console.error('Failed to load dashboard data:', error);
        window.Dashboard.showError('Failed to load project data. Please refresh the page.');
    }
};

window.Dashboard.renderDashboard = function(data) {
    if (!data) return;
    
    window.Dashboard.renderMetrics(data.metrics);
    window.Dashboard.renderMilestones(data.milestones);
    window.Dashboard.renderWorkItems(data.workItems);
};

window.Dashboard.renderMetrics = function(metrics) {
    const metricsContainer = document.querySelector('.metrics-grid');
    if (!metricsContainer || !metrics) return;
    
    metricsContainer.innerHTML = `
        <div class="metric-card success">
            <div class="metric-card-label">Completion</div>
            <div class="metric-card-value">${dashboardUtils.formatPercentage(metrics.completionPercentage)}</div>
            <div class="metric-card-detail">Project progress</div>
        </div>
        <div class="metric-card info">
            <div class="metric-card-label">Status</div>
            <div class="metric-card-value">${metrics.healthStatus || 'N/A'}</div>
            <div class="metric-card-detail">Current health</div>
        </div>
        <div class="metric-card warning">
            <div class="metric-card-label">Velocity</div>
            <div class="metric-card-value">${metrics.velocityThisMonth || 0}</div>
            <div class="metric-card-detail">Items this month</div>
        </div>
        <div class="metric-card info">
            <div class="metric-card-label">Milestones</div>
            <div class="metric-card-value">${metrics.milestonesCompleted || 0}/${metrics.milestonesTotal || 0}</div>
            <div class="metric-card-detail">Completed</div>
        </div>
    `;
};

window.Dashboard.renderMilestones = function(milestones) {
    const timelineContainer = document.querySelector('.timeline');
    if (!timelineContainer || !milestones) return;
    
    timelineContainer.innerHTML = milestones.map(m => `
        <div class="timeline-item ${m.status}">
            <div class="font-bold">${m.name}</div>
            <div class="text-muted" style="font-size: 0.875rem; margin-top: 0.5rem;">
                ${dashboardUtils.formatDate(m.targetDate)}
            </div>
        </div>
    `).join('');
};

window.Dashboard.renderWorkItems = function(workItems) {
    if (!workItems) return;
    
    // Group items by status: shipped, inprogress, carriedover
    const grouped = {
        shipped: [],
        inprogress: [],
        carriedover: []
    };
    
    workItems.forEach(item => {
        const status = (item.status || 'carriedover').toLowerCase();
        if (status === 'shipped') {
            grouped.shipped.push(item);
        } else if (status === 'inprogress') {
            grouped.inprogress.push(item);
        } else if (status === 'carriedover') {
            grouped.carriedover.push(item);
        }
    });
    
    const workItemsGrid = document.querySelector('.work-items-grid');
    if (!workItemsGrid) return;
    
    workItemsGrid.innerHTML = `
        <div class="work-item-column">
            <div class="work-item-column-header">
                <div class="work-item-count">${grouped.shipped.length}</div>
                <div>Shipped This Month</div>
            </div>
            ${grouped.shipped.map(item => `
                <div class="work-item">
                    <div class="work-item-title">${item.title}</div>
                    <div class="work-item-description">${item.description}</div>
                </div>
            `).join('')}
        </div>
        <div class="work-item-column">
            <div class="work-item-column-header">
                <div class="work-item-count">${grouped.inprogress.length}</div>
                <div>In Progress</div>
            </div>
            ${grouped.inprogress.map(item => `
                <div class="work-item">
                    <div class="work-item-title">${item.title}</div>
                    <div class="work-item-description">${item.description}</div>
                </div>
            `).join('')}
        </div>
        <div class="work-item-column">
            <div class="work-item-column-header">
                <div class="work-item-count">${grouped.carriedover.length}</div>
                <div>Carried Over</div>
            </div>
            ${grouped.carriedover.map(item => `
                <div class="work-item">
                    <div class="work-item-title">${item.title}</div>
                    <div class="work-item-description">${item.description}</div>
                </div>
            `).join('')}
        </div>
    `;
};

window.Dashboard.showError = function(message) {
    const app = document.getElementById('app');
    if (app) {
        app.innerHTML = `<div class="alert alert-danger m-5">${message}</div>`;
    }
};

// Load dashboard data when page ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.Dashboard.loadData);
} else {
    window.Dashboard.loadData();
}