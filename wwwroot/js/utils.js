// Dashboard Utility Functions

window.dashboardUtils = window.dashboardUtils || {};

dashboardUtils.formatDate = function(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
};

dashboardUtils.formatPercentage = function(value) {
    if (typeof value !== 'number') return '0%';
    return Math.round(value) + '%';
};

dashboardUtils.getStatusColor = function(status) {
    const colors = {
        'completed': '#10b981',
        'inprogress': '#3b82f6',
        'atrisk': '#ef4444',
        'future': '#d1d5db'
    };
    return colors[status.toLowerCase()] || '#9ca3af';
};

dashboardUtils.getStatusClass = function(status) {
    const classes = {
        'completed': 'success',
        'inprogress': 'info',
        'atrisk': 'danger',
        'future': 'secondary'
    };
    return classes[status.toLowerCase()] || 'secondary';
};