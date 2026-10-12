// Theme API used by ThemeService; persistence and applying live in prefs.js.
window.themeManager = {
    getTheme: function () {
        return window.prefs.get('theme');
    },

    setTheme: function (theme) {
        window.prefs.set('theme', theme);
    }
};
