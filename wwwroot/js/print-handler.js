// Print and screenshot optimization handler
window.printHandler = {
    preparePrintView: function() {
        // Remove scrollbars
        document.body.style.overflow = 'hidden';
        
        // Adjust viewport for printing
        const viewport = document.querySelector('meta[name="viewport"]');
        if (viewport) {
            viewport.setAttribute('content', 'width=device-width, initial-scale=1.0');
        }
    },
    
    restoreNormalView: function() {
        document.body.style.overflow = 'auto';
    },
    
    optimizeForScreenshot: function(width, height) {
        const root = document.documentElement;
        
        if (width === 1280 && height === 720) {
            root.style.setProperty('--breakpoint-tablet', '1024px');
            root.style.setProperty('--breakpoint-desktop', '1280px');
        } else if (width === 1920 && height === 1080) {
            root.style.setProperty('--breakpoint-tablet', '1024px');
            root.style.setProperty('--breakpoint-desktop', '1280px');
            root.style.setProperty('--breakpoint-presentation', '1920px');
        }
    }
};

// Print event listeners
window.addEventListener('beforeprint', function() {
    window.printHandler.preparePrintView();
});

window.addEventListener('afterprint', function() {
    window.printHandler.restoreNormalView();
});