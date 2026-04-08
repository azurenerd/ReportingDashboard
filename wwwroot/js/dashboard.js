/**
 * Dashboard Initialization and Timeline Rendering
 * Handles chart.js timeline initialization and responsive dashboard behavior
 */

(function () {
    'use strict';

    const Dashboard = {
        // Configuration
        config: {
            animationDuration: 500,
            responsiveDelay: 300,
            timelineChartId: 'timelineChart',
            timelineContainerId: 'timelineContainer'
        },

        // State
        state: {
            timelineChart: null,
            isInitialized: false,
            currentViewport: null
        },

        /**
         * Initialize dashboard on page load
         */
        init: function () {
            if (this.state.isInitialized) return;

            this.attachEventListeners();
            this.initializeTimeline();
            this.handleResponsiveResize();
            this.state.isInitialized = true;
        },

        /**
         * Attach event listeners for dashboard interactions
         */
        attachEventListeners: function () {
            window.addEventListener('resize', this.debounce(
                () => this.handleResponsiveResize(),
                this.config.responsiveDelay
            ));

            document.addEventListener('DOMContentLoaded', () => {
                this.initializeTimeline();
            });
        },

        /**
         * Initialize timeline chart using Chart.js
         */
        initializeTimeline: function () {
            const timelineContainer = document.getElementById(this.config.timelineContainerId);
            if (!timelineContainer) return;

            const canvas = timelineContainer.querySelector('canvas');
            if (!canvas) {
                console.warn('Timeline canvas element not found');
                return;
            }

            const ctx = canvas.getContext('2d');
            if (!ctx) {
                console.warn('Could not get canvas context');
                return;
            }

            // Get milestone data from data attribute or window object
            const milestoneData = this.getMilestoneData();
            if (!milestoneData || milestoneData.length === 0) {
                console.warn('No milestone data available');
                return;
            }

            this.renderTimeline(ctx, milestoneData);
        },

        /**
         * Render timeline chart
         */
        renderTimeline: function (ctx, milestones) {
            const labels = milestones.map(m => m.name);
            const datasets = this.buildTimelineDatasets(milestones);

            if (this.state.timelineChart) {
                this.state.timelineChart.destroy();
            }

            this.state.timelineChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: datasets
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    aspectRatio: 3,
                    plugins: {
                        legend: {
                            display: false
                        },
                        tooltip: {
                            enabled: true,
                            backgroundColor: 'rgba(0, 0, 0, 0.8)',
                            padding: 12,
                            titleFont: {
                                size: 14,
                                weight: 'bold'
                            },
                            bodyFont: {
                                size: 12
                            },
                            displayColors: true,
                            callbacks: {
                                label: function (context) {
                                    return context.dataset.label || '';
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            display: true,
                            beginAtZero: true,
                            max: 100,
                            ticks: {
                                callback: function (value) {
                                    return value + '%';
                                }
                            },
                            grid: {
                                color: 'rgba(0, 0, 0, 0.05)'
                            }
                        },
                        x: {
                            display: true,
                            grid: {
                                display: false
                            }
                        }
                    }
                }
            });
        },

        /**
         * Build timeline datasets for Chart.js
         */
        buildTimelineDatasets: function (milestones) {
            const statusColors = {
                'Completed': 'rgba(39, 174, 96, 1)',
                'InProgress': 'rgba(52, 152, 219, 1)',
                'AtRisk': 'rgba(231, 76, 60, 1)',
                'Future': 'rgba(149, 165, 166, 1)'
            };

            return milestones.map((milestone, index) => {
                const status = milestone.status || 'Future';
                const color = statusColors[status] || statusColors['Future'];

                return {
                    label: milestone.name,
                    data: Array(index + 1).fill(null).map((_, i) => (i + 1) * (100 / milestones.length)),
                    borderColor: color,
                    backgroundColor: color.replace('1)', '0.1)'),
                    borderWidth: 2,
                    fill: false,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    pointBackgroundColor: color,
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    tension: 0.3
                };
            });
        },

        /**
         * Get milestone data from page or API
         */
        getMilestoneData: function () {
            // Try to get from window object (set by Blazor component)
            if (window.dashboardData && window.dashboardData.milestones) {
                return window.dashboardData.milestones;
            }

            // Fallback: try to parse from data attribute
            const container = document.getElementById(this.config.timelineContainerId);
            if (container && container.dataset.milestones) {
                try {
                    return JSON.parse(container.dataset.milestones);
                } catch (e) {
                    console.error('Failed to parse milestone data:', e);
                }
            }

            return [];
        },

        /**
         * Handle responsive resize events
         */
        handleResponsiveResize: function () {
            const viewport = {
                width: window.innerWidth,
                height: window.innerHeight
            };

            // Check if viewport changed significantly
            if (this.state.currentViewport &&
                this.state.currentViewport.width === viewport.width &&
                this.state.currentViewport.height === viewport.height) {
                return;
            }

            this.state.currentViewport = viewport;
            this.applyResponsiveClasses(viewport);

            if (this.state.timelineChart) {
                this.state.timelineChart.resize();
            }
        },

        /**
         * Apply responsive CSS classes based on viewport
         */
        applyResponsiveClasses: function (viewport) {
            const root = document.documentElement;
            root.classList.remove('viewport-tablet', 'viewport-desktop', 'viewport-presentation');

            if (viewport.width >= 1920) {
                root.classList.add('viewport-presentation');
            } else if (viewport.width >= 1280) {
                root.classList.add('viewport-desktop');
            } else {
                root.classList.add('viewport-tablet');
            }
        },

        /**
         * Debounce function for resize events
         */
        debounce: function (func, wait) {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        },

        /**
         * Update dashboard metrics display
         */
        updateMetrics: function (metrics) {
            const metricCards = document.querySelectorAll('.metric-card');
            metricCards.forEach((card, index) => {
                const metric = metrics[index];
                if (metric) {
                    const valueElement = card.querySelector('.metric-value');
                    const subtextElement = card.querySelector('.metric-subtext');
                    if (valueElement) {
                        valueElement.textContent = metric.value;
                    }
                    if (subtextElement && metric.subtext) {
                        subtextElement.textContent = metric.subtext;
                    }
                }
            });
        },

        /**
         * Update work items display
         */
        updateWorkItems: function (items) {
            const columns = document.querySelectorAll('.work-item-column');
            columns.forEach((column, index) => {
                const list = column.querySelector('.work-item-list');
                if (list) {
                    list.innerHTML = '';
                    if (items[index]) {
                        items[index].forEach(item => {
                            const li = document.createElement('li');
                            li.className = 'work-item';
                            li.innerHTML = `
                                <h3 class="work-item-title">${this.escapeHtml(item.title)}</h3>
                                <p class="work-item-description">${this.escapeHtml(item.description || '')}</p>
                            `;
                            list.appendChild(li);
                        });
                    }
                }
            });
        },

        /**
         * Escape HTML to prevent XSS
         */
        escapeHtml: function (text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
    };

    // Initialize on document ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => Dashboard.init());
    } else {
        Dashboard.init();
    }

    // Expose to global scope for Blazor interop
    window.Dashboard = Dashboard;
})();