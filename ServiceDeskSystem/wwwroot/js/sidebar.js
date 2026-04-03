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
            const isCtrlB = (event.ctrlKey || event.metaKey) && event.key && event.key.toLowerCase() === 'b';
            if (!isCtrlB) {
                return;
            }

            event.preventDefault();
            dotNetRef.invokeMethodAsync('HandleSidebarHotkey');
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
