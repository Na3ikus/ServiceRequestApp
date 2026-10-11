// Kanban Drag & Drop JS Interop
// Created by Nazar "N1h3" Hutsol

window.kanban = {
    // Map of element -> AbortController for drag listeners, keyed by element reference
    _dragControllers: new WeakMap(),

    // Drop zone element -> { dotnetRef, status }, used by touch drag (no HTML5 drop events on touch screens)
    _zones: new WeakMap(),

    // Board container used for edge auto-scroll during touch drag
    _board: null,

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
        this._board = container;

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
        this._zones.set(element, { dotnetRef: dotnetRef, status: status });

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

        this._initTouchDrag(element, ticketId, status, signal);
    },

    /**
     * Touch screens never fire HTML5 drag events, so cards also get a touch drag:
     * a long press (LONG_PRESS_MS without moving) picks the card up, a ghost follows
     * the finger, the drop zone under it is highlighted and releasing over another
     * column reports the drop through the same OnTicketDropped call.
     * Moving before the long press ends is treated as a scroll and cancels the pickup.
     */
    _initTouchDrag: function (element, ticketId, status, signal) {
        const LONG_PRESS_MS = 300;
        const MOVE_TOLERANCE = 8;
        const EDGE_SIZE = 48;
        const MAX_SPEED = 14;
        const self = this;

        let timer = null;
        let startX = 0;
        let startY = 0;
        let ghost = null;
        let offsetX = 0;
        let offsetY = 0;
        let overZone = null;

        function zoneAt(x, y) {
            const hit = document.elementFromPoint(x, y);
            const zone = hit && hit.closest('.kanban-drop-zone');
            return zone && self._zones.has(zone) ? zone : null;
        }

        function setOverZone(zone) {
            if (zone === overZone) return;
            if (overZone) overZone.classList.remove('kanban-drag-over');
            overZone = zone;
            if (overZone) overZone.classList.add('kanban-drag-over');
        }

        function edgeScroll(x) {
            const board = self._board;
            if (!board) return;
            const rect = board.getBoundingClientRect();
            if (x < rect.left + EDGE_SIZE) {
                self._scrollSpeed = -MAX_SPEED;
            } else if (x > rect.right - EDGE_SIZE) {
                self._scrollSpeed = MAX_SPEED;
            } else {
                self._stopScroll();
                return;
            }
            self._scrollContainer = board;
            self._ensureScrollLoop();
        }

        function pickUp(touch) {
            const rect = element.getBoundingClientRect();
            offsetX = touch.clientX - rect.left;
            offsetY = touch.clientY - rect.top;
            ghost = element.cloneNode(true);
            ghost.setAttribute('aria-hidden', 'true');
            Object.assign(ghost.style, {
                position: 'fixed',
                left: rect.left + 'px',
                top: rect.top + 'px',
                width: rect.width + 'px',
                pointerEvents: 'none',
                zIndex: '1000',
                opacity: '0.9',
            });
            document.body.appendChild(ghost);
            element.classList.add('kanban-dragging');
            if (navigator.vibrate) navigator.vibrate(10);
        }

        // Returns true when a picked-up card was released (the caller then cancels the tap).
        function finish(touch) {
            clearTimeout(timer);
            timer = null;
            if (!ghost) return false;

            ghost.remove();
            ghost = null;
            element.classList.remove('kanban-dragging');
            self._stopScroll();

            const zone = touch ? zoneAt(touch.clientX, touch.clientY) : null;
            setOverZone(null);

            const target = zone && self._zones.get(zone);
            if (target && target.status !== status) {
                target.dotnetRef.invokeMethodAsync('OnTicketDropped', ticketId, target.status);
            }

            return true;
        }

        element.addEventListener('touchstart', function (e) {
            if (e.touches.length !== 1) return;
            const t = e.touches[0];
            startX = t.clientX;
            startY = t.clientY;
            timer = setTimeout(function () {
                timer = null;
                pickUp(t);
            }, LONG_PRESS_MS);
        }, { signal, passive: true });

        // Not passive: once a card is picked up, preventDefault stops the page from scrolling.
        element.addEventListener('touchmove', function (e) {
            const t = e.touches[0];
            if (!ghost) {
                if (timer && Math.hypot(t.clientX - startX, t.clientY - startY) > MOVE_TOLERANCE) {
                    clearTimeout(timer);
                    timer = null;
                }
                return;
            }

            e.preventDefault();
            ghost.style.left = (t.clientX - offsetX) + 'px';
            ghost.style.top = (t.clientY - offsetY) + 'px';
            setOverZone(zoneAt(t.clientX, t.clientY));
            edgeScroll(t.clientX);
        }, { signal, passive: false });

        // preventDefault on touchend stops the browser from turning the release into a click (opening the ticket).
        element.addEventListener('touchend', function (e) {
            if (finish(e.changedTouches[0])) e.preventDefault();
        }, { signal, passive: false });

        element.addEventListener('touchcancel', function () {
            finish(null);
        }, { signal });

        // The long press that picks a card up must not also open the context menu.
        element.addEventListener('contextmenu', function (e) {
            if (ghost) e.preventDefault();
        }, { signal });
    }
};
