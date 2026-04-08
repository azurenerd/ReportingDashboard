/**
 * Global Error Handler Module
 * Handles missing assets and graceful error recovery
 */

(function(window) {
    'use strict';

    // Error handler configuration
    var ErrorHandler = {
        isInitialized: false,
        errorQueue: [],
        maxErrors: 10,

        // Initialize global error handlers
        init: function() {
            if (this.isInitialized) return;
            
            try {
                // Global unhandled error listener
                window.addEventListener('error', this.handleError.bind(this), true);
                
                // Unhandled promise rejection listener
                window.addEventListener('unhandledrejection', this.handleUnhandledRejection.bind(this));
                
                // Asset load error listener (for CSS, JS, images)
                document.addEventListener('error', this.handleResourceError.bind(this), true);
                
                this.isInitialized = true;
                console.info('Global error handler initialized.');
            } catch (error) {
                console.error('Failed to initialize error handler:', error);
            }
        },

        // Handle global JavaScript errors
        handleError: function(event) {
            var error = {
                type: 'error',
                timestamp: new Date().toISOString(),
                message: event.message || 'Unknown error',
                filename: event.filename || 'unknown',
                lineno: event.lineno || 0,
                colno: event.colno || 0,
                stack: event.error ? event.error.stack : 'No stack trace'
            };
            
            this.logError(error);
            this.displayErrorNotification(error.message);
            
            // Prevent default error handling for certain errors
            if (this.isAssetLoadError(error)) {
                event.preventDefault();
            }
        },

        // Handle unhandled promise rejections
        handleUnhandledRejection: function(event) {
            var error = {
                type: 'unhandledRejection',
                timestamp: new Date().toISOString(),
                message: event.reason ? event.reason.toString() : 'Unhandled promise rejection',
                reason: event.reason
            };
            
            this.logError(error);
            this.displayErrorNotification('An asynchronous operation failed. Check console for details.');
            
            // Prevent default unhandled rejection handling
            event.preventDefault();
        },

        // Handle resource load errors (CSS, JS, images, etc.)
        handleResourceError: function(event) {
            if (event.target === window) return; // Ignore non-resource errors
            
            var target = event.target;
            var resourceType = target.tagName ? target.tagName.toLowerCase() : 'unknown';
            var resourceUrl = '';
            
            // Get resource URL based on type
            if (target.href) {
                resourceUrl = target.href; // Link element (CSS)
            } else if (target.src) {
                resourceUrl = target.src; // Script or img element
            }
            
            var error = {
                type: 'resourceError',
                timestamp: new Date().toISOString(),
                resourceType: resourceType,
                resourceUrl: resourceUrl,
                message: 'Failed to load ' + resourceType + ' asset: ' + resourceUrl
            };
            
            this.logError(error);
            
            // Special handling for critical assets
            if (this.isCriticalAsset(resourceType, resourceUrl)) {
                this.displayErrorNotification('Failed to load critical resource: ' + resourceUrl);
            }
        },

        // Determine if error is related to asset loading
        isAssetLoadError: function(error) {
            return error.filename && (
                error.filename.includes('.css') ||
                error.filename.includes('.js') ||
                error.message.includes('CORS')
            );
        },

        // Determine if asset is critical to dashboard functionality
        isCriticalAsset: function(resourceType, url) {
            var criticalPatterns = [
                'dashboard.css',
                'pico.css',
                'chart.js',
                'error-handler.js',
                'timeline.js'
            ];
            
            return criticalPatterns.some(function(pattern) {
                return url.includes(pattern);
            });
        },

        // Log error to internal queue
        logError: function(error) {
            try {
                this.errorQueue.push(error);
                
                // Trim queue if too large
                if (this.errorQueue.length > this.maxErrors) {
                    this.errorQueue.shift();
                }
                
                // Log to console with formatting
                console.error('[Dashboard Error]', error);
            } catch (e) {
                console.error('Failed to log error:', e);
            }
        },

        // Display user-friendly error notification
        displayErrorNotification: function(message) {
            try {
                // Check if error notification already visible
                var existingNotification = document.getElementById('dashboard-error-notification');
                if (existingNotification) {
                    return; // Don't stack notifications
                }
                
                // Create notification element
                var notification = document.createElement('div');
                notification.id = 'dashboard-error-notification';
                notification.style.cssText = [
                    'position: fixed',
                    'top: 20px',
                    'right: 20px',
                    'background-color: #fef2f2',
                    'border: 2px solid #e74c3c',
                    'border-radius: 4px',
                    'padding: 16px',
                    'color: #991b1b',
                    'font-size: 14px',
                    'max-width: 400px',
                    'box-shadow: 0 2px 8px rgba(0,0,0,0.15)',
                    'z-index: 9999',
                    'font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif'
                ].join(';');
                
                notification.innerHTML = '<strong>Dashboard Error:</strong> ' + this.escapeHtml(message);
                
                // Add close button
                var closeBtn = document.createElement('button');
                closeBtn.textContent = '✕';
                closeBtn.style.cssText = [
                    'position: absolute',
                    'top: 8px',
                    'right: 8px',
                    'background: none',
                    'border: none',
                    'color: #991b1b',
                    'cursor: pointer',
                    'font-size: 18px',
                    'padding: 0',
                    'width: 24px',
                    'height: 24px'
                ].join(';');
                closeBtn.onclick = function() {
                    notification.remove();
                };
                notification.appendChild(closeBtn);
                
                document.body.appendChild(notification);
                
                // Auto-dismiss after 8 seconds
                setTimeout(function() {
                    try {
                        notification.remove();
                    } catch (e) {
                        // Already removed
                    }
                }, 8000);
            } catch (error) {
                console.error('Failed to display error notification:', error);
            }
        },

        // Escape HTML special characters for safe display
        escapeHtml: function(text) {
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, function(m) {
                return map[m];
            });
        },

        // Get all logged errors
        getErrors: function() {
            return this.errorQueue.slice();
        },

        // Clear error log
        clearErrors: function() {
            this.errorQueue = [];
        },

        // Check if any errors were logged
        hasErrors: function() {
            return this.errorQueue.length > 0;
        }
    };

    // Export to global scope
    window.ErrorHandler = ErrorHandler;

    // Auto-initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            ErrorHandler.init();
        });
    } else {
        // DOM already loaded
        ErrorHandler.init();
    }

    console.info('Error handler module loaded.');
})(window);