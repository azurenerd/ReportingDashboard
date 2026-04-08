/**
 * Print Handler and Screenshot Optimization
 * Optimizes dashboard for printing and screenshot capture
 */

(function () {
    'use strict';

    const PrintHandler = {
        // Configuration
        config: {
            printMediaQueryList: window.matchMedia('print'),
            screenshotModeAttribute: 'data-screenshot-mode',
            scrollbarHideClass: 'scrollbar-hidden',
            printOptimizedClass: 'print-optimized'
        },

        // State
        state: {
            originalBodyOverflow: null,
            originalHtmlOverflow: null,
            isPrintMode: false
        },

        /**
         * Initialize print handler
         */
        init: function () {
            this.attachPrintListeners();
            this.addPrintStyles();
        },

        /**
         * Attach print media query listeners
         */
        attachPrintListeners: function () {
            // Listen for print mode changes
            this.config.printMediaQueryList.addListener((mql) => {
                if (mql.matches) {
                    this.onPrintStart();
                } else {
                    this.onPrintEnd();
                }
            });

            // Listen for print button/keyboard shortcuts
            window.addEventListener('beforeprint', () => this.onPrintStart());
            window.addEventListener('afterprint', () => this.onPrintEnd());
        },

        /**
         * Handle print start
         */
        onPrintStart: function () {
            this.state.isPrintMode = true;
            document.documentElement.classList.add(this.config.printOptimizedClass);
            this.hideScrollbars();
            this.optimizeForCapture();
        },

        /**
         * Handle print end
         */
        onPrintEnd: function () {
            this.state.isPrintMode = false;
            document.documentElement.classList.remove(this.config.printOptimizedClass);
            this.showScrollbars();
            this.restoreNormalView();
        },

        /**
         * Hide scrollbars
         */
        hideScrollbars: function () {
            const html = document.documentElement;
            const body = document.body;

            this.state.originalHtmlOverflow = html.style.overflow;
            this.state.originalBodyOverflow = body.style.overflow;

            html.style.overflow = 'hidden';
            body.style.overflow = 'hidden';

            // Hide all scrollable containers
            const scrollableContainers = document.querySelectorAll('.timeline-container, .work-item-list');
            scrollableContainers.forEach(container => {
                container.style.overflow = 'visible';
                container.classList.add(this.config.scrollbarHideClass);
            });
        },

        /**
         * Show scrollbars
         */
        showScrollbars: function () {
            const html = document.documentElement;
            const body = document.body;

            if (this.state.originalHtmlOverflow !== null) {
                html.style.overflow = this.state.originalHtmlOverflow;
            }
            if (this.state.originalBodyOverflow !== null) {
                body.style.overflow = this.state.originalBodyOverflow;
            }

            // Restore scrollable containers
            const scrollableContainers = document.querySelectorAll('.timeline-container, .work-item-list');
            scrollableContainers.forEach(container => {
                container.style.overflow = '';
                container.classList.remove(this.config.scrollbarHideClass);
            });
        },

        /**
         * Optimize layout for screenshot capture
         */
        optimizeForCapture: function () {
            // Remove any floating elements that might cause clipping
            const floatingElements = document.querySelectorAll('button, input, select, textarea');
            floatingElements.forEach(element => {
                element.style.display = 'none';
            });

            // Adjust timeline container for horizontal scrolling content
            const timelineContainer = document.querySelector('.timeline-container');
            if (timelineContainer) {
                timelineContainer.style.flexWrap = 'wrap';
                timelineContainer.style.gap = '8pt';
            }

            // Adjust work item columns
            const workItemColumns = document.querySelectorAll('.work-item-column');
            workItemColumns.forEach(column => {
                column.style.maxHeight = 'none';
                column.style.overflow = 'visible';
            });

            // Disable transitions during print
            document.documentElement.style.transition = 'none !important';
            const allElements = document.querySelectorAll('*');
            allElements.forEach(el => {
                el.style.transition = 'none !important';
            });
        },

        /**
         * Restore normal view
         */
        restoreNormalView: function () {
            // Restore floating elements
            const floatingElements = document.querySelectorAll('button, input, select, textarea');
            floatingElements.forEach(element => {
                element.style.display = '';
            });

            // Restore timeline container
            const timelineContainer = document.querySelector('.timeline-container');
            if (timelineContainer) {
                timelineContainer.style.flexWrap = '';
                timelineContainer.style.gap = '';
            }

            // Restore work item columns
            const workItemColumns = document.querySelectorAll('.work-item-column');
            workItemColumns.forEach(column => {
                column.style.maxHeight = '';
                column.style.overflow = '';
            });

            // Re-enable transitions
            document.documentElement.style.transition = '';
            const allElements = document.querySelectorAll('*');
            allElements.forEach(el => {
                el.style.transition = '';
            });
        },

        /**
         * Add print-specific styles to document
         */
        addPrintStyles: function () {
            const styleId = 'print-handler-styles';
            if (document.getElementById(styleId)) return;

            const styles = `
                @media print {
                    html, body {
                        overflow: hidden !important;
                    }

                    .scrollbar-hidden {
                        overflow: visible !important;
                    }

                    button, input, select, textarea, .no-print {
                        display: none !important;
                    }
                }

                .print-optimized {
                    overflow: hidden;
                }

                .print-optimized .timeline-container,
                .print-optimized .work-item-list {
                    overflow: visible !important;
                }
            `;

            const style = document.createElement('style');
            style.id = styleId;
            style.textContent = styles;
            document.head.appendChild(style);
        },

        /**
         * Trigger print dialog
         */
        printDashboard: function () {
            window.print();
        },

        /**
         * Take screenshot for PowerPoint (triggers print dialog)
         */
        captureScreenshot: function () {
            console.log('Capturing screenshot for PowerPoint export...');
            this.printDashboard();
        },

        /**
         * Optimize for specific viewport size (1280x720 or 1920x1080)
         */
        optimizeForViewport: function (width, height) {
            const root = document.documentElement;
            const body = document.body;

            // Set explicit dimensions
            root.style.width = width + 'px';
            root.style.height = height + 'px';
            body.style.width = width + 'px';
            body.style.height = height + 'px';

            // Remove excess padding/margins for capture
            root.style.padding = '0';
            body.style.padding = '0';
            body.style.margin = '0';

            // Force specific font sizing for consistency
            root.style.fontSize = height >= 1080 ? '18px' : '16px';
        },

        /**
         * Reset viewport optimization
         */
        resetViewport: function () {
            const root = document.documentElement;
            const body = document.body;

            root.style.width = '';
            root.style.height = '';
            body.style.width = '';
            body.style.height = '';
            root.style.padding = '';
            body.style.padding = '';
            body.style.margin = '';
            root.style.fontSize = '';
        }
    };

    // Initialize on document ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => PrintHandler.init());
    } else {
        PrintHandler.init();
    }

    // Expose to global scope for manual invocation
    window.PrintHandler = PrintHandler;
})();