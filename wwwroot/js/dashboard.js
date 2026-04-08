// ============================================================================
// DASHBOARD INITIALIZATION AND CLIENT-SIDE UTILITIES
// ============================================================================
// Handles dashboard setup, timeline initialization, and data binding for
// executive dashboard components. Coordinates with Blazor components for
// real-time data updates and Chart.js visualization.
// ============================================================================

(function() {
    'use strict';

    // Dashboard initialization on DOM content loaded
    document.addEventListener('DOMContentLoaded', function() {
        console.log('[Dashboard] Initializing dashboard on page load');
        initializeDashboard();
    });

    // Initialize dashboard components
    function initializeDashboard() {
        console.log('[Dashboard] Dashboard initialization started');
        
        // Set up event listeners
        setupEventListeners();
        
        // Initialize accessible components
        setupAccessibility();
        
        console.log('[Dashboard] Dashboard initialization complete');
    }

    // Event listener setup
    function setupEventListeners() {
        // Window resize handler for responsive adjustments
        window.addEventListener('resize', debounce(function() {
            console.log('[Dashboard] Window resized, triggering layout adjustment');
            adjustLayoutForViewport();
        }, 250));
        
        // Print preparation
        window.addEventListener('beforeprint', function() {
            console.log('[Dashboard] Preparing for print');
            preparePrintView();
        });
        
        window.addEventListener('afterprint', function() {
            console.log('[Dashboard] Restoring normal view after print');
            restoreNormalView();
        });
    }

    // Setup accessibility features
    function setupAccessibility() {
        // Ensure keyboard navigation support
        document.querySelectorAll('[data-interactive]').forEach(function(el) {
            if (!el.hasAttribute('tabindex')) {
                el.setAttribute('tabindex', '0');
            }
            
            el.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    el.click();
                }
            });
        });
    }

    // Adjust layout for current viewport
    function adjustLayoutForViewport() {
        var width = window.innerWidth;
        var height = window.innerHeight;
        
        console.log('[Dashboard] Viewport: ' + width + 'x' + height);
        
        if (width < 1024) {
            document.body.classList.add('compact-layout');
        } else {
            document.body.classList.remove('compact-layout');
        }
    }

    // Prepare view for printing
    function preparePrintView() {
        document.body.style.overflow = 'hidden';
        document.querySelectorAll('.dashboard-wrapper').forEach(function(el) {
            el.style.overflow = 'visible';
        });
    }

    // Restore normal view after printing
    function restoreNormalView() {
        document.body.style.overflow = 'auto';
        document.querySelectorAll('.dashboard-wrapper').forEach(function(el) {
            el.style.overflow = 'auto';
        });
    }

    // Timeline Chart Configuration
    window.dashboardCharts = {
        initializeTimeline: function(canvasId, data) {
            console.log('[Dashboard] Initializing timeline chart: ' + canvasId);
            
            var ctx = document.getElementById(canvasId);
            if (!ctx) {
                console.warn('[Dashboard] Canvas element not found: ' + canvasId);
                return null;
            }
            
            // Return chart instance for external control
            return new Chart(ctx, {
                type: 'bar',
                data: data,
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    indexAxis: 'y',
                    plugins: {
                        legend: {
                            display: false
                        },
                        tooltip: {
                            callbacks: {
                                label: function(context) {
                                    return context.raw + '%';
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
                            }
                        }
                    }
                }
            });
        }
    };

    // Date formatting utility
    window.dashboardUtils = {
        formatDate: function(dateString) {
            var date = new Date(dateString);
            var options = { year: 'numeric', month: 'short', day: 'numeric' };
            return date.toLocaleDateString('en-US', options);
        },
        
        formatDateTime: function(dateString) {
            var date = new Date(dateString);
            var options = { 
                year: 'numeric', 
                month: 'short', 
                day: 'numeric', 
                hour: '2-digit', 
                minute: '2-digit'
            };
            return date.toLocaleDateString('en-US', options);
        },
        
        getMonthYear: function(dateString) {
            var date = new Date(dateString);
            var options = { year: 'numeric', month: 'long' };
            return date.toLocaleDateString('en-US', options);
        },
        
        getDaysSinceOrUntil: function(dateString) {
            var targetDate = new Date(dateString);
            var today = new Date();
            today.setHours(0, 0, 0, 0);
            targetDate.setHours(0, 0, 0, 0);
            
            var diffTime = targetDate - today;
            var diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
            
            return diffDays;
        }
    };

    // Utility function: debounce
    function debounce(func, wait) {
        var timeout;
        return function executedFunction() {
            var later = function() {
                clearTimeout(timeout);
                func();
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Expose init function for manual trigger if needed
    window.reinitializeDashboard = function() {
        console.log('[Dashboard] Manual reinitialization triggered');
        initializeDashboard();
    };

})();