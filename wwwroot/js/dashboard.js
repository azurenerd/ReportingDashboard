// Executive Dashboard - Main utilities
(function () {
    window.Dashboard = window.Dashboard || {};

    Dashboard.initialize = function () {
        console.log('Dashboard initialized');
        Dashboard.setupEventListeners();
        Dashboard.setupPrintOptimization();
    };

    Dashboard.setupEventListeners = function () {
        // Handle window resize for responsive layout adjustments
        window.addEventListener('resize', function () {
            Dashboard.onViewportChange();
        });

        // Handle print preview
        window.addEventListener('beforeprint', function () {
            Dashboard.onPrintStart();
        });

        window.addEventListener('afterprint', function () {
            Dashboard.onPrintEnd();
        });
    };

    Dashboard.setupPrintOptimization = function () {
        // Ensure print media queries are active
        var mediaQueryList = window.matchMedia('print');
        mediaQueryList.addListener(function (mql) {
            if (mql.matches) {
                Dashboard.onPrintStart();
            } else {
                Dashboard.onPrintEnd();
            }
        });
    };

    Dashboard.onViewportChange = function () {
        var width = window.innerWidth;
        var height = window.innerHeight;

        if (width < 1024) {
            console.warn('Viewport below minimum width (1024px): ' + width);
        }

        // Trigger layout recalculation for responsive components
        if (window.Charts && typeof window.Charts.recalculate === 'function') {
            Charts.recalculate();
        }
    };

    Dashboard.onPrintStart = function () {
        console.log('Print mode activated');
        document.body.classList.add('print-mode');
    };

    Dashboard.onPrintEnd = function () {
        console.log('Print mode deactivated');
        document.body.classList.remove('print-mode');
    };

    Dashboard.getViewportInfo = function () {
        return {
            width: window.innerWidth,
            height: window.innerHeight,
            isTablet: window.innerWidth >= 1024 && window.innerWidth < 1280,
            isDesktop: window.innerWidth >= 1280 && window.innerWidth < 1920,
            isWidescreen: window.innerWidth >= 1920,
            isPrintMode: window.matchMedia('print').matches
        };
    };

    Dashboard.formatDate = function (date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    };

    Dashboard.formatPercentage = function (value) {
        return Math.round(value) + '%';
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            Dashboard.initialize();
        });
    } else {
        Dashboard.initialize();
    }
})();