// Kanban Drag & Drop JS Interop
// Created by Nazar "N1h3" Hutsol

window.kanban = {
    // Map of element -> AbortController for drag listeners, keyed by element reference
    _dragControllers: new WeakMap(),

    // Auto-scroll state
    _scrollRAF: null,
    _scrollContainer: null,
    _scrollSpeed: 0,

    /**
     * Initialize the kanban board container for horizontal auto-scroll during drag.
     * Call once after the board renders. When dragging near the left/right edge
     * of the scroll container the board will smoothly scroll in that direction.
     * @param {HTMLElement} container - The scrollable kanban board wrapper element
     */
    initBoard: function (container) {
        if (!container) return;

        const EDGE_SIZE = 120;   // px from edge that triggers scroll
        const MAX_SPEED = 18;    // max px per animation frame

        const self = this;

        container.addEventListener('dragover', function (e) {
            const rect = container.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const width = rect.width;

            if (x < EDGE_SIZE) {
                // Near left edge
                self._scrollSpeed = -MAX_SPEED * (1 - x / EDGE_SIZE);
                self._scrollContainer = container;
                self._ensureScrollLoop();
            } else if (x > width - EDGE_SIZE) {
                // Near right edge
                self._scrollSpeed = MAX_SPEED * (1 - (width - x) / EDGE_SIZE);
                self._scrollContainer = container;
                self._ensureScrollLoop();
            } else {
                self._stopScroll();
            }
        });

        container.addEventListener('dragleave', function (e) {
            if (!container.contains(e.relatedTarget)) {
                self._stopScroll();
            }
        });

        container.addEventListener('drop', function () {
            self._stopScroll();
        });

        container.addEventListener('dragend', function () {
            self._stopScroll();
        });
    },

    _ensureScrollLoop: function () {
        if (this._scrollRAF) return; // already running
        const self = this;

        function loop() {
            if (self._scrollContainer && self._scrollSpeed !== 0) {
                self._scrollContainer.scrollLeft += self._scrollSpeed;
                self._scrollRAF = requestAnimationFrame(loop);
            } else {
                self._scrollRAF = null;
            }
        }

        self._scrollRAF = requestAnimationFrame(loop);
    },

    _stopScroll: function () {
        this._scrollSpeed = 0;
        this._scrollContainer = null;
        if (this._scrollRAF) {
            cancelAnimationFrame(this._scrollRAF);
            this._scrollRAF = null;
        }
    },

    /**
     * Initialize drag-and-drop for a kanban column drop zone.
     * Drop zones are registered only once (firstRender), so no cleanup needed here.
     * @param {HTMLElement} element - The column drop zone element
     * @param {DotNetObjectReference} dotnetRef - Blazor .NET reference
     * @param {string} status - The status this column represents
     */
    initDropZone: function (element, dotnetRef, status) {
        if (!element) return;

        element.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            element.classList.add('kanban-drag-over');
        });

        element.addEventListener('dragleave', function (e) {
            if (!element.contains(e.relatedTarget)) {
                element.classList.remove('kanban-drag-over');
            }
        });

        element.addEventListener('drop', function (e) {
            e.preventDefault();
            element.classList.remove('kanban-drag-over');

            const ticketId = parseInt(e.dataTransfer.getData('text/ticketId'));
            const sourceStatus = e.dataTransfer.getData('text/sourceStatus');

            if (!isNaN(ticketId) && sourceStatus !== status) {
                dotnetRef.invokeMethodAsync('OnTicketDropped', ticketId, status);
            }
        });
    },

    /**
     * Initialize a draggable card element.
     * Aborts any previously registered drag listeners on this element
     * before registering new ones — this is essential because Blazor
     * may re-render the card with a new status without recreating the
     * DOM element, so we need to update the captured 'status' closure.
     * @param {HTMLElement} element - The card element
     * @param {number} ticketId - The ticket ID
     * @param {string} status - Current ticket status
     */
    initDraggable: function (element, ticketId, status) {
        if (!element) return;

        // Abort previous listeners if any
        const existing = this._dragControllers.get(element);
        if (existing) {
            existing.abort();
        }

        const controller = new AbortController();
        const signal = controller.signal;
        this._dragControllers.set(element, controller);

        element.setAttribute('draggable', 'true');

        element.addEventListener('dragstart', function (e) {
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/ticketId', ticketId.toString());
            e.dataTransfer.setData('text/sourceStatus', status);
            element.classList.add('kanban-dragging');
        }, { signal });

        element.addEventListener('dragend', function (e) {
            element.classList.remove('kanban-dragging');
        }, { signal });
    }
};
