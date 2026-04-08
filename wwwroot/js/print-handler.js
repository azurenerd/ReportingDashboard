// ============================================================================
// PRINT AND SCREENSHOT OPTIMIZATION HANDLER
// ============================================================================
// Manages print/screenshot capture workflow, optimizes viewport sizing,
// and ensures clean output for PowerPoint capture at 1280x720 and 1920x1080.
// ============================================================================

(function() {
    'use strict';

    // Expose print handler API globally
    window.printHandler = {
        
        // Prepare dashboard for print/screenshot capture
        preparePrintView: function() {
            console.log('[PrintHandler] Preparing print view');
            
            // Hide scrollbars
            document.body.style.overflow = 'hidden';
            document.documentElement.style.overflow = 'hidden';
            
            // Remove any interactive overlays
            document.querySelectorAll('[data-no-print]').forEach(function(el) {
                el.style.display = 'none';
            });
            
            // Ensure full viewport utilization
            document.querySelectorAll('.dashboard-wrapper').forEach(function(el) {
                el.style.overflow = 'visible';
                el.style.height = 'auto';
            });
            
            // Disable transitions for clean capture
            var style = document.createElement('style');
            style.textContent = '* { transition: none !important; animation: none !important; }';
            style.id = 'print-disable-animations';
            document.head.appendChild(style);
        },
        
        // Restore normal view after print/screenshot
        restoreNormalView: function() {
            console.log('[PrintHandler] Restoring normal view');
            
            document.body.style.overflow = 'auto';
            document.documentElement.style.overflow = 'auto';
            
            document.querySelectorAll('[data-no-print]').forEach(function(el) {
                el.style.display = '';
            });
            
            // Remove animation disable style
            var style = document.getElementById('print-disable-animations');
            if (style) {
                style.remove();
            }
        },
        
        // Optimize viewport for specific screenshot resolution
        optimizeForScreenshot: function(width, height) {
            console.log('[PrintHandler] Optimizing for screenshot: ' + width + 'x' + height);
            
            var root = document.documentElement;
            
            if (width === 1280 && height === 720) {
                console.log('[PrintHandler] Applying 1280x720 optimization');
                root.style.setProperty('--viewport-width', width + 'px');
                root.style.setProperty('--viewport-height', height + 'px');
                
                // Compact metric cards
                root.style.setProperty('--spacing-lg', '16px');
                root.style.setProperty('--spacing-md', '12px');
                
                // Reduce font sizes slightly
                root.style.setProperty('--font-size-base', '13px');
            } 
            else if (width === 1920 && height === 1080) {
                console.log('[PrintHandler] Applying 1920x1080 optimization');
                root.style.setProperty('--viewport-width', width + 'px');
                root.style.setProperty('--viewport-height', height + 'px');
                
                // Standard spacing for presentation
                root.style.setProperty('--spacing-lg', '28px');
                root.style.setProperty('--spacing-md', '16px');
                
                // Standard font sizes
                root.style.setProperty('--font-size-base', '16px');
            }
            
            // Trigger layout recalculation
            void document.body.offsetHeight;
        },
        
        // Remove sidebar/navigation for clean screenshot
        hideNavigation: function() {
            console.log('[PrintHandler] Hiding navigation elements');
            
            document.querySelectorAll('nav, .sidebar, .navigation').forEach(function(el) {
                el.style.display = 'none';
            });
        },
        
        // Show navigation again
        showNavigation: function() {
            console.log('[PrintHandler] Showing navigation elements');
            
            document.querySelectorAll('nav, .sidebar, .navigation').forEach(function(el) {
                el.style.display = '';
            });
        },
        
        // Capture dashboard snapshot
        captureScreenshot: function(filename) {
            console.log('[PrintHandler] Capturing screenshot: ' + (filename || 'dashboard'));
            
            // Note: This would require a library like html2canvas
            // For now, we just log the intent
            console.warn('[PrintHandler] Screenshot capture requires html2canvas library');
        },
        
        // Trigger browser print dialog
        printDashboard: function() {
            console.log('[PrintHandler] Triggering print dialog');
            
            this.preparePrintView();
            
            // Allow layout to settle before printing
            setTimeout(function() {
                window.print();
            }, 100);
        }
    };

    // Hook into browser print events
    window.addEventListener('beforeprint', function() {
        console.log('[PrintHandler] Browser beforeprint event');
        window.printHandler.preparePrintView();
    });

    window.addEventListener('afterprint', function() {
        console.log('[PrintHandler] Browser afterprint event');
        window.printHandler.restoreNormalView();
    });

    // Keyboard shortcut for print (Ctrl+P or Cmd+P already handled by browser)
    // But we can enhance with custom logic if needed
    document.addEventListener('keydown', function(e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'p') {
            // Browser will handle print dialog, but we prepare anyway
            console.log('[PrintHandler] Print shortcut detected');
        }
    });

    // Expose common print operations
    window.printDashboard = function() {
        window.printHandler.printDashboard();
    };

    window.optimizeForScreenshot = function(width, height) {
        window.printHandler.optimizeForScreenshot(width, height);
    };

})();