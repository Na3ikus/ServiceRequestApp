window.sidebarManager = {
    registerHotkey: function (dotNetRef) {
        this.unregisterHotkey();

        window.__sidebarHotkeyHandler = function (event) {
            const tag = (event.target.tagName || '').toLowerCase();
            const isInput = tag === 'input' || tag === 'textarea' || tag === 'select' || event.target.isContentEditable;

            // Application keyboard shortcuts (only when not typing and no dialog is open)
            if (isInput || event.ctrlKey || event.metaKey || event.altKey) {
                return;
            }

            if (document.querySelector('[aria-modal="true"]')) {
                return;
            }

            const kbEnabled = localStorage.getItem('settings.keyboardShortcuts');
            if (kbEnabled === 'false') {
                return;
            }

            // Global hotkeys using event.code to ignore physical keyboard layout (UA/EN etc)
            const code = event.code;

            if (code === 'KeyN') {
                event.preventDefault();
                dotNetRef.invokeMethodAsync('HandleNavigateHotkey', '/create-ticket');
            } else if (code === 'KeyT') {
                event.preventDefault();
                // Route to the correct tickets page based on the user's role
                var userRole = localStorage.getItem('user.role');
                var targetUrl = (userRole && userRole.toLowerCase() === 'user') ? '/my-tickets' : '/tickets';
                dotNetRef.invokeMethodAsync('HandleNavigateHotkey', targetUrl);
            } else if (code === 'Slash' && !event.shiftKey) {
                event.preventDefault();
                const searchInput = document.querySelector('input[type="search"]');
                if (searchInput) {
                    searchInput.focus();
                    searchInput.select();
                }
            } else if (code === 'KeyD') {
                event.preventDefault();
                dotNetRef.invokeMethodAsync('HandleThemeHotkey');
            } else if ((code === 'Slash' && event.shiftKey) || code === 'KeyH') {
                // "?" (Shift + /) or "H" - open hotkeys help
                event.preventDefault();
                dotNetRef.invokeMethodAsync('HandleHotkeysHelp');
            }
        };

        document.addEventListener('keydown', window.__sidebarHotkeyHandler);

        window.__showHotkeysHelpHandler = function () {
            dotNetRef.invokeMethodAsync('HandleHotkeysHelp');
        };
        document.addEventListener('show-hotkeys-help', window.__showHotkeysHelpHandler);
    },

    showHotkeysHelp: function () {
        document.dispatchEvent(new CustomEvent('show-hotkeys-help'));
    },

    unregisterHotkey: function () {
        if (!window.__sidebarHotkeyHandler) {
            return;
        }

        document.removeEventListener('keydown', window.__sidebarHotkeyHandler);
        window.__sidebarHotkeyHandler = null;

        if (window.__showHotkeysHelpHandler) {
            document.removeEventListener('show-hotkeys-help', window.__showHotkeysHelpHandler);
            window.__showHotkeysHelpHandler = null;
        }
    }
};

// Close header dropdown details when clicking outside
document.addEventListener('click', function (event) {
    var details = document.querySelectorAll('details.header-dropdown');
    details.forEach(function (detail) {
        if (detail.hasAttribute('open') && !detail.contains(event.target)) {
            detail.removeAttribute('open');
        }
    });
});
