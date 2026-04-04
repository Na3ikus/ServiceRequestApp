// Theme management with localStorage persistence and debounced transitions
window.themeManager = {
    _transitionTimer: null,
    _isTransitioning: false,

    getTheme: function () {
        return localStorage.getItem('theme') || 'light';
    },
    
    setTheme: function (theme) {
        localStorage.setItem('theme', theme);

        // Cancel any pending transition cleanup from previous rapid clicks
        if (this._transitionTimer) {
            clearTimeout(this._transitionTimer);
            this._transitionTimer = null;
        }

        // Enable smooth color transitions for the theme switch
        if (!this._isTransitioning) {
            document.documentElement.classList.add('theme-transitioning');
            this._isTransitioning = true;
        }

        // Apply the theme class immediately
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        
        // Remove transition class after animation completes.
        // Using a single debounced timer ensures rapid clicks don't leave
        // the class stuck — only the LAST click's timer fires.
        this._transitionTimer = setTimeout(() => {
            document.documentElement.classList.remove('theme-transitioning');
            this._isTransitioning = false;
            this._transitionTimer = null;
        }, 400);
    },
    
    initialize: function () {
        const theme = this.getTheme();
        // On initial load, apply theme instantly without transitions
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        return theme;
    }
};

// Initialize theme on page load (no transition on first load)
window.themeManager.initialize();
