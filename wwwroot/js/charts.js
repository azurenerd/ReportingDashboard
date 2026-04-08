// Chart rendering and visualization utilities
(function () {
    window.Charts = window.Charts || {};

    Charts.renderTimeline = function (elementId, milestones) {
        var container = document.getElementById(elementId);
        if (!container) {
            console.error('Timeline container not found: ' + elementId);
            return;
        }

        container.innerHTML = '';
        
        if (!milestones || milestones.length === 0) {
            container.innerHTML = '<p>No milestones available</p>';
            return;
        }

        milestones.forEach(function (milestone, index) {
            var item = Charts.createTimelineItem(milestone, index);
            container.appendChild(item);
        });

        Charts.adjustTimelineWidth(container);
    };

    Charts.createTimelineItem = function (milestone, index) {
        var item = document.createElement('div');
        item.className = 'timeline-item ' + milestone.status.toLowerCase();
        item.setAttribute('data-index', index);

        var marker = document.createElement('div');
        marker.className = 'timeline-marker';

        var content = document.createElement('div');
        content.className = 'timeline-content';

        var title = document.createElement('div');
        title.className = 'timeline-title';
        title.textContent = milestone.name;

        var date = document.createElement('div');
        date.className = 'timeline-date';
        date.textContent = Charts.formatDateShort(milestone.targetDate);

        content.appendChild(title);
        content.appendChild(date);

        item.appendChild(marker);
        item.appendChild(content);

        return item;
    };

    Charts.renderProgressBar = function (elementId, percentage, status) {
        var container = document.getElementById(elementId);
        if (!container) {
            console.error('Progress container not found: ' + elementId);
            return;
        }

        container.innerHTML = '';
        var progressBar = document.createElement('div');
        progressBar.className = 'progress';

        var fill = document.createElement('div');
        fill.className = 'progress-bar ' + (status || 'info');
        fill.style.width = percentage + '%';
        fill.textContent = Charts.formatPercentage(percentage);

        progressBar.appendChild(fill);
        container.appendChild(progressBar);
    };

    Charts.renderMetricCard = function (elementId, value, label, suffix) {
        var container = document.getElementById(elementId);
        if (!container) {
            console.error('Metric container not found: ' + elementId);
            return;
        }

        container.innerHTML = '';
        var metric = document.createElement('div');
        metric.className = 'metric';

        var valueDiv = document.createElement('div');
        valueDiv.className = 'metric-value';
        valueDiv.textContent = value + (suffix || '');

        var labelDiv = document.createElement('div');
        labelDiv.className = 'metric-label';
        labelDiv.textContent = label;

        metric.appendChild(valueDiv);
        metric.appendChild(labelDiv);
        container.appendChild(metric);
    };

    Charts.adjustTimelineWidth = function (container) {
        var viewportInfo = window.Dashboard.getViewportInfo();
        var width = viewportInfo.width;

        if (width < 1024) {
            container.style.overflowX = 'auto';
        } else if (width >= 1024 && width < 1920) {
            container.style.overflowX = 'hidden';
        } else {
            container.style.overflowX = 'hidden';
        }
    };

    Charts.formatDateShort = function (date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }
        return date.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric'
        });
    };

    Charts.formatPercentage = function (value) {
        return Math.round(value) + '%';
    };

    Charts.recalculate = function () {
        // Recalculate chart dimensions on viewport change
        var timelines = document.querySelectorAll('[data-chart-type="timeline"]');
        timelines.forEach(function (timeline) {
            Charts.adjustTimelineWidth(timeline);
        });
    };

    Charts.getStatusClass = function (status) {
        switch (status.toLowerCase()) {
            case 'completed':
                return 'success';
            case 'in-progress':
                return 'info';
            case 'at-risk':
                return 'warning';
            case 'future':
                return 'neutral';
            default:
                return 'neutral';
        }
    };
})();