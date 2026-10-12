// Modal focus trap: while a dialog is open Tab / Shift+Tab cycle inside it,
// and closing it gives focus back to the element focused before it opened.
// Traps are keyed by an id because Blazor removes the dialog before release;
// they stack, and only the most recently activated one acts.
// Listeners sit on the document (capture) so the trap still works when focus
// has fallen out of the dialog, e.g. to <body> when the focused Save button
// gets disabled or after a click on the overlay.
// The logic only needs a few DOM members, so it is tested under Node with plain objects.
(function () {
    'use strict';

    const FOCUSABLE = [
        'a[href]',
        'button',
        'input:not([type="hidden"])',
        'select',
        'textarea',
        '[tabindex]',
        '[contenteditable="true"]',
    ].join(',');

    function rendered(el) {
        if (el.getClientRects().length === 0) return false;
        const style = typeof globalThis.getComputedStyle === 'function' ? globalThis.getComputedStyle(el) : null;
        return !style || style.visibility !== 'hidden';
    }

    // Tabbable elements in DOM order: no tabindex="-1", nothing disabled (also by a
    // disabled fieldset), inert, hidden or visibility: hidden.
    function focusables(container) {
        return Array.from(container.querySelectorAll(FOCUSABLE)).filter((el) =>
            el.tabIndex >= 0 && !el.matches(':disabled') && !el.closest('[inert]') && rendered(el));
    }

    // Where Tab has to send focus to stay inside: the other end when leaving
    // from the first/last element or from outside the list (e.g. <body>), the
    // container when nothing is tabbable, otherwise null (the browser's move is fine).
    function wrapTarget(items, active, backwards, container) {
        if (items.length === 0) return container;

        const first = items[0];
        const last = items[items.length - 1];
        const inside = items.includes(active);

        if (backwards) return !inside || active === first ? last : null;
        return !inside || active === last ? first : null;
    }

    // Focus in another open dialog (the command palette over a modal) is left alone.
    function inOtherDialog(container, el) {
        return Boolean(el && el.closest && !container.contains(el) && el.closest('[aria-modal="true"]'));
    }

    function onKeyDown(container, event, doc) {
        const active = doc.activeElement;
        if (inOtherDialog(container, active)) return;

        if (event.key === 'Tab') {
            const target = wrapTarget(focusables(container), active, event.shiftKey, container);
            if (target) {
                event.preventDefault();
                target.focus();
            }
            return;
        }

        // Focus fell out of the dialog: Esc would reach <body> and not the dialog's
        // own keydown handler, so bring focus back and replay the key there.
        if (event.key === 'Escape' && !container.contains(active)) {
            event.preventDefault();
            event.stopPropagation();
            container.focus();
            container.dispatchEvent(new globalThis.KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
        }
    }

    function onFocusIn(container, event) {
        const target = event.target;
        if (container.contains(target) || inOtherDialog(container, target)) return;

        (focusables(container)[0] || container).focus();
    }

    // ---- Browser

    const traps = [];

    function top() {
        return traps[traps.length - 1];
    }

    const handleKeyDown = (event) => {
        const trap = top();
        if (trap) onKeyDown(trap.container, event, globalThis.document);
    };

    const handleFocusIn = (event) => {
        const trap = top();
        if (trap) onFocusIn(trap.container, event);
    };

    function activate(container, id) {
        if (!container) return;
        release(id, false);

        const doc = globalThis.document;
        if (traps.length === 0) {
            doc.addEventListener('keydown', handleKeyDown, true);
            doc.addEventListener('focusin', handleFocusIn, true);
        }

        traps.push({ id, container, previous: doc.activeElement });
        (focusables(container)[0] || container).focus();
    }

    function release(id, restore = true) {
        const index = traps.findIndex((t) => t.id === id);
        if (index < 0) return;

        const [trap] = traps.splice(index, 1);
        if (traps.length === 0) {
            const doc = globalThis.document;
            doc.removeEventListener('keydown', handleKeyDown, true);
            doc.removeEventListener('focusin', handleFocusIn, true);
        }

        const previous = trap.previous;
        if (restore && previous && previous.isConnected && typeof previous.focus === 'function') {
            previous.focus();
        }
    }

    const api = { FOCUSABLE, focusables, wrapTarget, onKeyDown, onFocusIn, activate, release };

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = api;
    }
    if (typeof window !== 'undefined') {
        window.focusTrap = api;
    }
})();
