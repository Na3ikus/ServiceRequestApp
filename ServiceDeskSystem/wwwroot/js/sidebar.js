window.sidebarManager = {
    getCollapsed: function () {
        return localStorage.getItem('sidebarCollapsed') === 'true';
    },

    setCollapsed: function (isCollapsed) {
        localStorage.setItem('sidebarCollapsed', isCollapsed ? 'true' : 'false');
    },

    isDesktop: function () {
        return window.matchMedia('(min-width: 1024px)').matches;
    },

    registerHotkey: function (dotNetRef) {
        this.unregisterHotkey();

        window.__sidebarHotkeyHandler = function (event) {
            const tag = (event.target.tagName || '').toLowerCase();
            const isInput = tag === 'input' || tag === 'textarea' || tag === 'select' || event.target.isContentEditable;

            // Ctrl+B — sidebar collapse
            const isCtrlB = (event.ctrlKey || event.metaKey) && event.key && event.key.toLowerCase() === 'b';
            if (isCtrlB) {
                event.preventDefault();
                dotNetRef.invokeMethodAsync('HandleSidebarHotkey');
                return;
            }

            // Application keyboard shortcuts (only when not typing)
            if (isInput || event.ctrlKey || event.metaKey || event.altKey) {
                return;
            }

            const kbEnabled = localStorage.getItem('settings.keyboardShortcuts');
            if (kbEnabled === 'false') {
                return;
            }

            const key = event.key && event.key.toLowerCase();

            if (key === 'n') {
                event.preventDefault();
                window.location.href = '/create-ticket';
            } else if (key === 't') {
                event.preventDefault();
                window.location.href = '/tickets';
            } else if (key === '/') {
                event.preventDefault();
                const searchInput = document.querySelector('.search-input');
                if (searchInput) {
                    searchInput.focus();
                    searchInput.select();
                }
            } else if (key === 'd') {
                event.preventDefault();
                dotNetRef.invokeMethodAsync('HandleThemeHotkey');
            }
        };

        document.addEventListener('keydown', window.__sidebarHotkeyHandler);
    },

    unregisterHotkey: function () {
        if (!window.__sidebarHotkeyHandler) {
            return;
        }

        document.removeEventListener('keydown', window.__sidebarHotkeyHandler);
        window.__sidebarHotkeyHandler = null;
    }
};

// Initialize accent color from localStorage on page load
(function () {
    var accent = localStorage.getItem('settings.accentColor');
    if (accent && accent !== 'blue') {
        document.documentElement.setAttribute('data-accent', accent);
    }
})();

