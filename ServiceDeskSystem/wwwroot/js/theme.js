// Theme management with localStorage persistence
window.themeManager = {
    getTheme: function () {
        return localStorage.getItem('theme') || 'light';
    },
    
    setTheme: function (theme) {
        localStorage.setItem('theme', theme);
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    },
    
    initialize: function () {
        const theme = this.getTheme();
        this.setTheme(theme);
        return theme;
    }
};

// Initialize theme on page load
window.themeManager.initialize();
