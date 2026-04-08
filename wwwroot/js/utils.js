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

dashboardUtils.formatNumber = function(value) {
    if (typeof value !== 'number') return '0';
    return value.toLocaleString('en-US');
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
        'blocked': '#dc2626',
        'future': '#d1d5db',
        'ontrack': '#10b981',
        'attest': '#f59e0b'
    };
    return colors[status.toLowerCase()] || '#9ca3af';
};

dashboardUtils.getStatusClass = function(status) {
    const classes = {
        'completed': 'success',
        'inprogress': 'info',
        'atrisk': 'danger',
        'blocked': 'danger',
        'future': 'secondary',
        'ontrack': 'success',
        'attest': 'warning'
    };
    return classes[status.toLowerCase()] || 'secondary';
};

dashboardUtils.truncateText = function(text, maxLength) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
};

dashboardUtils.calculateDaysRemaining = function(targetDate) {
    const today = new Date();
    const target = new Date(targetDate);
    const diff = target - today;
    const days = Math.ceil(diff / (1000 * 60 * 60 * 24));
    return days;
};

dashboardUtils.getHealthStatus = function(completion, daysRemaining) {
    if (completion >= 100) return 'completed';
    if (daysRemaining < 0) return 'atrisk';
    if (daysRemaining < 7 && completion < 80) return 'atrisk';
    if (daysRemaining < 14 && completion < 60) return 'attest';
    return 'ontrack';
};

dashboardUtils.debounce = function(func, wait) {
    let timeout;
    return function() {
        clearTimeout(timeout);
        const args = arguments;
        timeout = setTimeout(function() {
            func.apply(this, args);
        }, wait);
    };
};

dashboardUtils.reportError = function(message, error) {
    console.error('[Dashboard Error]', message, error);
    const appDiv = document.getElementById('app');
    if (appDiv) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger m-5';
        errorDiv.innerHTML = '<h5>' + message + '</h5><p>' + (error?.message || 'Unknown error') + '</p>';
        appDiv.appendChild(errorDiv);
    }
};