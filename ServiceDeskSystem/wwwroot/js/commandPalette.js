// Command palette: Ctrl+K toggle, keeping the selection in view, and pointer hover.
(function () {
    'use strict';

    const palette = {
        _dotNetRef: null,
        _x: null,
        _y: null,
        _index: -1,
        _keyboard: false,

        register(dotNetRef) {
            this._dotNetRef = dotNetRef;

            globalThis.document.addEventListener('keydown', (e) => {
                this.onKeyDown();
                // Ctrl+K or Cmd+K
                if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'K')) {
                    e.preventDefault();
                    e.stopPropagation();
                    this._dotNetRef?.invokeMethodAsync('Toggle');
                }
            });
            globalThis.document.addEventListener('pointermove', (e) => this.onPointerMove(e));
        },

        // Keys move the selection, so the row under the pointer no longer matches it.
        onKeyDown() {
            this._keyboard = true;
        },

        // Hover selects a row only on a real pointer move: browsers also fire pointer/mouse
        // events when the list scrolls under a still cursor, which would undo keyboard selection.
        onPointerMove(e) {
            if (e.clientX === this._x && e.clientY === this._y) return;
            this._x = e.clientX;
            this._y = e.clientY;

            const row = e.target?.closest?.('[data-cmd-index]');
            if (!row) return;

            const index = Number(row.getAttribute('data-cmd-index'));
            if (index === this._index && !this._keyboard) return;

            this._index = index;
            this._keyboard = false;
            this._dotNetRef?.invokeMethodAsync('HoverItem', index);
        },

        // Keeps the keyboard-selected option visible in the scrolling list.
        scrollToSelected(list) {
            list?.querySelector('[aria-selected="true"]')?.scrollIntoView({ block: 'nearest' });
        }
    };

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = palette;
    }
    if (typeof window !== 'undefined') {
        window.commandPalette = palette;
    }
})();
