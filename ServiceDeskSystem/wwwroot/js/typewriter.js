// Login panel typewriter: types the phrases from data-phrases one character at
// a time, holds, erases and moves on. The loop itself (initial/next/order) is
// pure so it can be tested under Node; start/stop drive it in the browser.
(function () {
    'use strict';

    const TYPE_MS = 55;
    const ERASE_MS = 25;
    const HOLD_MS = 3000;
    const GAP_MS = 400;

    function initial() {
        return { index: 0, text: '', phase: 'typing', delay: 0 };
    }

    // The frame after `state`: one more character, the hold, one less character
    // or the switch to the next phrase.
    function next(state, phrases) {
        const target = phrases[state.index % phrases.length];

        if (state.phase === 'typing') {
            if (state.text.length < target.length) {
                return { ...state, text: target.slice(0, state.text.length + 1), delay: TYPE_MS };
            }
            return { ...state, phase: 'erasing', delay: HOLD_MS };
        }

        if (state.text.length > 0) {
            return { ...state, text: state.text.slice(0, -1), delay: ERASE_MS };
        }

        return { index: (state.index + 1) % phrases.length, text: '', phase: 'typing', delay: GAP_MS };
    }

    // The first phrase (the tagline) stays first; the rest are shuffled.
    function order(phrases, random) {
        const rest = phrases.slice(1);
        for (let i = rest.length - 1; i > 0; i--) {
            const j = Math.floor(random() * (i + 1));
            [rest[i], rest[j]] = [rest[j], rest[i]];
        }
        return [phrases[0], ...rest];
    }

    // ---- Browser

    const timers = new Map();

    function reducedMotion() {
        return globalThis.document.documentElement.getAttribute('data-motion') === 'reduced';
    }

    function start(el) {
        stop(el);

        const output = el.querySelector('[data-typewriter-text]');
        let raw = el.getAttribute('data-phrases');
        let phrases = order(JSON.parse(raw), Math.random);

        if (reducedMotion()) {
            output.textContent = phrases[0];
            return;
        }

        let state = initial();
        const tick = () => {
            // Blazor rewrites data-phrases when the language changes.
            const current = el.getAttribute('data-phrases');
            if (current !== raw) {
                raw = current;
                phrases = order(JSON.parse(raw), Math.random);
                state = initial();
            }

            state = next(state, phrases);
            output.textContent = state.text;
            timers.set(el, setTimeout(tick, state.delay));
        };

        tick();
    }

    function stop(el) {
        clearTimeout(timers.get(el));
        timers.delete(el);
    }

    const api = { TYPE_MS, ERASE_MS, HOLD_MS, GAP_MS, initial, next, order, start, stop };

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = api;
    }
    if (typeof window !== 'undefined') {
        window.typewriter = api;
    }
})();
