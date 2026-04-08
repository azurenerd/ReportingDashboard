// Utility functions for dashboard

window.dashboardUtils = {
    formatDate: function(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { 
            year: 'numeric', 
            month: 'short', 
            day: 'numeric' 
        });
    },

    formatPercentage: function(value) {
        return Math.round(value) + '%';
    },

    getStatusClass: function(status) {
        const statusMap = {
            'completed': 'success',
            'inprogress': 'info',
            'atrisk': 'danger',
            'future': 'neutral'
        };
        return statusMap[status.toLowerCase()] || 'neutral';
    },

    getStatusColor: function(status) {
        const colorMap = {
            'completed': '#10b981',
            'inprogress': '#3b82f6',
            'atrisk': '#ef4444',
            'future': '#d1d5db'
        };
        return colorMap[status.toLowerCase()] || '#d1d5db';
    }
};