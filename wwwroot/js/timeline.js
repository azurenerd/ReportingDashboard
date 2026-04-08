/**
 * Timeline Visualization Module
 * Handles Chart.js initialization and milestone timeline rendering
 */

(function(window) {
    'use strict';

    // Timeline module object
    var Timeline = {
        // Initialize timeline chart with milestone data
        initTimeline: function(milestonesData, containerId) {
            containerId = containerId || 'timeline-chart';
            
            // Validate prerequisites
            if (typeof Chart === 'undefined') {
                console.error('Chart.js is not available. Timeline visualization cannot be rendered.');
                return this.renderTimelineTableFallback(milestonesData, containerId);
            }
            
            if (!milestonesData || !Array.isArray(milestonesData) || milestonesData.length === 0) {
                console.warn('No milestone data provided for timeline visualization.');
                return false;
            }
            
            try {
                var container = document.getElementById(containerId);
                if (!container) {
                    console.error('Timeline container not found:', containerId);
                    return false;
                }
                
                // Parse milestone data
                var chartData = this.prepareMilestoneChartData(milestonesData);
                
                // Create Chart.js instance
                var ctx = container.getContext('2d');
                if (!ctx) {
                    console.error('Unable to get canvas context for timeline.');
                    return false;
                }
                
                var chart = new Chart(ctx, {
                    type: 'scatter',
                    data: chartData,
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: true,
                                position: 'top'
                            }
                        },
                        scales: {
                            x: {
                                type: 'linear',
                                position: 'bottom',
                                title: {
                                    display: true,
                                    text: 'Timeline'
                                }
                            },
                            y: {
                                display: false
                            }
                        }
                    }
                });
                
                console.info('Timeline chart initialized successfully.');
                window.timelineChart = chart;
                return true;
            } catch (error) {
                console.error('Error initializing timeline chart:', error);
                return false;
            }
        },

        // Prepare milestone data for Chart.js
        prepareMilestoneChartData: function(milestones) {
            var statusColors = {
                'Completed': '#2ecc71',
                'InProgress': '#3498db',
                'AtRisk': '#e74c3c',
                'Future': '#95a5a6'
            };
            
            var datasets = [];
            var statusGroups = {};
            
            // Group milestones by status
            milestones.forEach(function(milestone, index) {
                var status = milestone.status || 'Future';
                if (!statusGroups[status]) {
                    statusGroups[status] = [];
                }
                statusGroups[status].push({
                    index: index,
                    name: milestone.name,
                    targetDate: new Date(milestone.targetDate).getTime(),
                    description: milestone.description || ''
                });
            });
            
            // Create dataset for each status
            var datasetIndex = 0;
            Object.keys(statusGroups).forEach(function(status) {
                var groupData = statusGroups[status];
                datasets.push({
                    label: status,
                    data: groupData.map(function(m, idx) {
                        return {
                            x: m.targetDate,
                            y: datasetIndex + idx * 0.5,
                            name: m.name,
                            description: m.description
                        };
                    }),
                    backgroundColor: statusColors[status] || '#95a5a6',
                    borderColor: statusColors[status] || '#95a5a6',
                    pointRadius: 8,
                    pointHoverRadius: 10,
                    borderWidth: 2
                });
                datasetIndex++;
            });
            
            return {
                datasets: datasets
            };
        },

        // Fallback table rendering if Chart.js unavailable
        renderTimelineTableFallback: function(milestones, containerId) {
            console.warn('Rendering timeline as HTML table (Chart.js unavailable).');
            
            var container = document.getElementById(containerId);
            if (!container) return false;
            
            try {
                var table = document.createElement('table');
                table.style.width = '100%';
                table.style.borderCollapse = 'collapse';
                
                // Header
                var thead = document.createElement('thead');
                var headerRow = document.createElement('tr');
                headerRow.style.backgroundColor = '#1a3a52';
                headerRow.style.color = '#ffffff';
                
                var headers = ['Milestone', 'Target Date', 'Status', 'Description'];
                headers.forEach(function(headerText) {
                    var th = document.createElement('th');
                    th.textContent = headerText;
                    th.style.padding = '8px';
                    th.style.textAlign = 'left';
                    th.style.borderBottom = '1px solid #ddd';
                    headerRow.appendChild(th);
                });
                thead.appendChild(headerRow);
                table.appendChild(thead);
                
                // Body
                var tbody = document.createElement('tbody');
                milestones.forEach(function(milestone) {
                    var row = document.createElement('tr');
                    row.style.borderBottom = '1px solid #ddd';
                    
                    var statusColorMap = {
                        'Completed': '#f0fdf4',
                        'InProgress': '#f0f9ff',
                        'AtRisk': '#fef2f2',
                        'Future': '#f8f9fa'
                    };
                    row.style.backgroundColor = statusColorMap[milestone.status] || '#ffffff';
                    
                    // Milestone name
                    var nameCell = document.createElement('td');
                    nameCell.textContent = milestone.name || 'N/A';
                    nameCell.style.padding = '8px';
                    row.appendChild(nameCell);
                    
                    // Target date
                    var dateCell = document.createElement('td');
                    dateCell.textContent = new Date(milestone.targetDate).toLocaleDateString() || 'N/A';
                    dateCell.style.padding = '8px';
                    row.appendChild(dateCell);
                    
                    // Status
                    var statusCell = document.createElement('td');
                    statusCell.textContent = milestone.status || 'Future';
                    statusCell.style.padding = '8px';
                    statusCell.style.fontWeight = '600';
                    row.appendChild(statusCell);
                    
                    // Description
                    var descCell = document.createElement('td');
                    descCell.textContent = milestone.description || '';
                    descCell.style.padding = '8px';
                    row.appendChild(descCell);
                    
                    tbody.appendChild(row);
                });
                table.appendChild(tbody);
                
                container.innerHTML = '';
                container.appendChild(table);
                
                console.info('Timeline table rendered successfully.');
                return true;
            } catch (error) {
                console.error('Error rendering timeline table:', error);
                return false;
            }
        },

        // Destroy existing timeline chart
        destroyTimeline: function() {
            try {
                if (window.timelineChart && typeof window.timelineChart.destroy === 'function') {
                    window.timelineChart.destroy();
                    window.timelineChart = null;
                    console.info('Timeline chart destroyed.');
                    return true;
                }
            } catch (error) {
                console.error('Error destroying timeline chart:', error);
            }
            return false;
        },

        // Check if Chart.js is available
        isAvailable: function() {
            return typeof Chart !== 'undefined' && window.chartJsAvailable === true;
        }
    };

    // Export module to global scope
    window.Timeline = Timeline;
    
    console.info('Timeline module initialized.');
})(window);