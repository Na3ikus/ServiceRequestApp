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
        }, 220);
    },
    
    initialize: function () {
        const theme = this.getTheme();
        // On initial load, apply theme instantly without transitions
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        this.restoreAllVisualSettings();
        return theme;
    },

    restoreAllVisualSettings: function () {
        try {
            const keys = [
                ['settings.borderRadius', 'data-radius', 'rounded'],
                ['settings.fontFamily', 'data-font', 'inter'],
                ['settings.contentWidth', 'data-content-width', 'standard'],
                ['settings.sidebarColor', 'data-sidebar-color', 'default'],
                ['settings.pageTransition', 'data-transition', 'fade'],
                ['settings.contrastMode', 'data-contrast', 'standard'],
                ['settings.badgeStyle', 'data-badge-style', 'filled'],
                ['settings.notifPosition', 'data-notif-position', 'top-right'],
                ['settings.bgTexture', 'data-bg-texture', 'none']
            ];

            for (let i = 0; i < keys.length; i++) {
                const [storageKey, attr, defaultVal] = keys[i];
                const val = localStorage.getItem(storageKey) || defaultVal;
                document.documentElement.setAttribute(attr, val);
            }

            const customHex = localStorage.getItem('settings.customAccentHex');
            const accentMode = localStorage.getItem('settings.accentColor');
            if (customHex && accentMode === 'custom') {
                document.documentElement.setAttribute('data-accent', 'custom');
                document.documentElement.style.setProperty('--accent-custom', customHex);
            } else if (accentMode) {
                document.documentElement.setAttribute('data-accent', accentMode);
            }
        } catch (e) {
            // Ignore localStorage errors
        }
    }
};

// Initialize theme on page load (no transition on first load)
window.themeManager.initialize();
