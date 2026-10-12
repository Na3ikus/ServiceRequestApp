// Modal focus trap: while a dialog is open Tab / Shift+Tab cycle inside it,
// and closing it gives focus back to the element focused before it opened.
// Traps are keyed by an id because Blazor removes the dialog before release.
// The Tab logic (focusables/wrapTarget/onKeyDown) only needs querySelectorAll,
// getClientRects and focus, so it is tested under Node with plain objects.
(function () {
    'use strict';

    const FOCUSABLE = [
        'a[href]',
        'button:not([disabled])',
        'input:not([disabled]):not([type="hidden"])',
        'select:not([disabled])',
        'textarea:not([disabled])',
        '[tabindex]:not([tabindex="-1"])',
        '[contenteditable="true"]',
    ].join(',');

    // Tabbable elements in DOM order, skipping hidden ones (no layout box).
    function focusables(container) {
        return Array.from(container.querySelectorAll(FOCUSABLE))
            .filter((el) => el.getClientRects().length > 0);
    }

    // Where Tab has to send focus to stay inside: the other end when leaving
    // from the first/last element or from outside the list, the container when
    // nothing is tabbable, otherwise null (the browser's own move is fine).
    function wrapTarget(items, active, backwards, container) {
        if (items.length === 0) return container;

        const first = items[0];
        const last = items[items.length - 1];
        const inside = items.includes(active);

        if (backwards) return !inside || active === first ? last : null;
        return !inside || active === last ? first : null;
    }

    function onKeyDown(container, event, doc) {
        if (event.key !== 'Tab') return;

        const target = wrapTarget(focusables(container), doc.activeElement, event.shiftKey, container);
        if (target) {
            event.preventDefault();
            target.focus();
        }
    }

    // ---- Browser

    const traps = new Map();

    function activate(container, id) {
        if (!container) return;
        release(id, false);

        const doc = globalThis.document;
        const previous = doc.activeElement;
        const handler = (event) => onKeyDown(container, event, doc);

        container.addEventListener('keydown', handler);
        traps.set(id, { container, previous, handler });

        const items = focusables(container);
        (items[0] || container).focus();
    }

    function release(id, restore = true) {
        const trap = traps.get(id);
        if (!trap) return;

        traps.delete(id);
        trap.container.removeEventListener('keydown', trap.handler);

        const previous = trap.previous;
        if (restore && previous && previous.isConnected && typeof previous.focus === 'function') {
            previous.focus();
        }
    }

    const api = { FOCUSABLE, focusables, wrapTarget, onKeyDown, activate, release };

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = api;
    }
    if (typeof window !== 'undefined') {
        window.focusTrap = api;
    }
})();
