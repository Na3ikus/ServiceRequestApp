const test = require('node:test');
const assert = require('node:assert/strict');
const focusTrap = require('../focusTrap.js');

// Minimal DOM stand-ins: what the trap touches and nothing else. focus() fires
// focusin on the document like a browser does.
function fakeDoc() {
    const listeners = { keydown: [], focusin: [] };
    return {
        activeElement: null,
        listeners,
        addEventListener(type, fn, capture) { assert.equal(capture, true); listeners[type].push(fn); },
        removeEventListener(type, fn) { listeners[type] = listeners[type].filter((f) => f !== fn); },
        fire(type, event) { [...listeners[type]].forEach((fn) => fn(event)); },
    };
}

function el(doc, name, { visible = true, hidden = false, tabIndex = 0, disabled = false, inert = false, dialog = null } = {}) {
    return {
        name,
        tabIndex,
        isConnected: true,
        visibility: hidden ? 'hidden' : 'visible',
        getClientRects: () => (visible ? [{}] : []),
        matches: (selector) => selector === ':disabled' && disabled,
        closest(selector) {
            if (selector === '[inert]') return inert ? {} : null;
            if (selector === '[aria-modal="true"]') return dialog;
            return null;
        },
        focus() {
            doc.activeElement = this;
            doc.fire('focusin', { target: this });
        },
    };
}

function container(doc, items) {
    const box = el(doc, 'box', { tabIndex: -1 });
    box.items = items;
    box.dispatched = [];
    box.querySelectorAll = () => box.items;
    box.contains = (node) => node === box || box.items.includes(node);
    box.dispatchEvent = (event) => box.dispatched.push(event);
    return box;
}

function key(name, shiftKey = false) {
    return {
        key: name,
        shiftKey,
        prevented: false,
        stopped: false,
        preventDefault() { this.prevented = true; },
        stopPropagation() { this.stopped = true; },
    };
}

// Installs a fake document (and KeyboardEvent) for activate/release.
function withDocument(t) {
    const doc = fakeDoc();
    globalThis.document = doc;
    globalThis.KeyboardEvent = class {
        constructor(type, init) { Object.assign(this, { type }, init); }
    };
    doc.cleanup = []; // releases left open by a test, run before the document goes away
    t.after(() => {
        doc.cleanup.forEach((fn) => fn());
        delete globalThis.document;
        delete globalThis.KeyboardEvent;
        delete globalThis.getComputedStyle;
    });
    return doc;
}

test('lists only visible, enabled, tabbable elements in order', (t) => {
    const doc = withDocument(t);
    globalThis.getComputedStyle = (node) => ({ visibility: node.visibility });
    const items = [
        el(doc, 'a'),
        el(doc, 'not rendered', { visible: false }),
        el(doc, 'visibility hidden', { hidden: true }),
        el(doc, 'tabindex -1', { tabIndex: -1 }),
        el(doc, 'disabled (or in a disabled fieldset)', { disabled: true }),
        el(doc, 'inert', { inert: true }),
        el(doc, 'b'),
    ];

    assert.deepEqual(focusTrap.focusables(container(doc, items)).map((x) => x.name), ['a', 'b']);
});

test('wraps Tab from the last element to the first and Shift+Tab back', () => {
    const doc = fakeDoc();
    const [a, b, c] = ['a', 'b', 'c'].map((n) => el(doc, n));

    assert.equal(focusTrap.wrapTarget([a, b, c], c, false), a);
    assert.equal(focusTrap.wrapTarget([a, b, c], a, true), c);
    assert.equal(focusTrap.wrapTarget([a, b, c], a, false), null, 'inner moves are left to the browser');
    assert.equal(focusTrap.wrapTarget([a, b, c], c, true), null);
});

test('keeps focus on the container when nothing inside is tabbable', () => {
    const doc = fakeDoc();
    const box = container(doc, []);

    assert.equal(focusTrap.wrapTarget([], null, false, box), box);
});

test('activate focuses the first element, release restores the opener', (t) => {
    const doc = withDocument(t);
    const opener = el(doc, 'opener');
    opener.focus();
    const [a, b] = ['a', 'b'].map((n) => el(doc, n));
    const box = container(doc, [a, b]);

    focusTrap.activate(box, 'm1');
    assert.equal(doc.activeElement, a);

    b.focus();
    const event = key('Tab');
    doc.fire('keydown', event);
    assert.equal(event.prevented, true);
    assert.equal(doc.activeElement, a, 'Tab on the last element wraps to the first');

    focusTrap.release('m1');
    assert.equal(doc.activeElement, opener);
    assert.equal(doc.listeners.keydown.length, 0);
    assert.equal(doc.listeners.focusin.length, 0);

    focusTrap.release('m1'); // second release is a no-op
    assert.equal(doc.activeElement, opener);
});

test('Tab and Shift+Tab from <body> go back into the dialog', (t) => {
    const doc = withDocument(t);
    const body = el(doc, 'body');
    const [a, b] = ['a', 'b'].map((n) => el(doc, n));
    focusTrap.activate(container(doc, [a, b]), 'm2');
    doc.cleanup.push(() => focusTrap.release('m2', false));

    // e.g. the focused Save button was disabled while saving: focus drops to <body>.
    doc.activeElement = body;
    const tab = key('Tab');
    doc.fire('keydown', tab);
    assert.equal(tab.prevented, true);
    assert.equal(doc.activeElement, a);

    doc.activeElement = body;
    doc.fire('keydown', key('Tab', true));
    assert.equal(doc.activeElement, b);
});

test('Esc with focus outside the dialog is replayed on the dialog', (t) => {
    const doc = withDocument(t);
    const body = el(doc, 'body');
    const box = container(doc, [el(doc, 'a')]);
    focusTrap.activate(box, 'm3');
    doc.cleanup.push(() => focusTrap.release('m3', false));

    doc.activeElement = body; // after a click on the overlay
    const esc = key('Escape');
    doc.fire('keydown', esc);

    assert.equal(esc.prevented, true);
    assert.equal(esc.stopped, true);
    assert.equal(doc.activeElement, box);
    assert.deepEqual(box.dispatched.map((e) => [e.type, e.key, e.bubbles]), [['keydown', 'Escape', true]]);
});

test('Esc inside the dialog is left to the dialog', (t) => {
    const doc = withDocument(t);
    const a = el(doc, 'a');
    const box = container(doc, [a]);
    focusTrap.activate(box, 'm4');
    doc.cleanup.push(() => focusTrap.release('m4', false));

    const esc = key('Escape');
    doc.fire('keydown', esc);

    assert.equal(esc.prevented, false);
    assert.equal(box.dispatched.length, 0);
    assert.equal(doc.activeElement, a);
});

test('focus moving outside is pulled back in', (t) => {
    const doc = withDocument(t);
    const sidebarLink = el(doc, 'sidebar link');
    const a = el(doc, 'a');
    focusTrap.activate(container(doc, [a]), 'm5');
    doc.cleanup.push(() => focusTrap.release('m5', false));

    sidebarLink.focus();

    assert.equal(doc.activeElement, a);
});

test('focus in another open dialog (command palette) is left alone', (t) => {
    const doc = withDocument(t);
    const palette = {};
    const paletteInput = el(doc, 'palette input', { dialog: palette });
    const a = el(doc, 'a');
    const box = container(doc, [a]);
    focusTrap.activate(box, 'm6');
    doc.cleanup.push(() => focusTrap.release('m6', false));

    paletteInput.focus();
    const tab = key('Tab');
    doc.fire('keydown', tab);
    const esc = key('Escape');
    doc.fire('keydown', esc);

    assert.equal(doc.activeElement, paletteInput);
    assert.equal(tab.prevented, false);
    assert.equal(box.dispatched.length, 0);
});

test('only the most recent of stacked traps acts; releasing it hands back to the outer one', (t) => {
    const doc = withDocument(t);
    const outerButton = el(doc, 'outer button');
    const outer = container(doc, [outerButton]);
    const innerButton = el(doc, 'inner button');
    const inner = container(doc, [innerButton]);

    focusTrap.activate(outer, 'outer');
    doc.cleanup.push(() => focusTrap.release('outer', false));
    focusTrap.activate(inner, 'inner');
    assert.equal(doc.activeElement, innerButton);
    assert.equal(doc.listeners.keydown.length, 1, 'one document listener for all traps');

    outerButton.focus();
    assert.equal(doc.activeElement, innerButton, 'the outer dialog is outside the inner trap');

    focusTrap.release('inner');
    assert.equal(doc.activeElement, outerButton, 'focus returns to the opener in the outer dialog');

    el(doc, 'page').focus();
    assert.equal(doc.activeElement, outerButton, 'the outer trap acts again');
});

test('activate focuses the dialog itself when it has nothing tabbable', (t) => {
    const doc = withDocument(t);
    const box = container(doc, []);

    focusTrap.activate(box, 'm7');
    assert.equal(doc.activeElement, box);
    focusTrap.release('m7');
});

test('release skips an opener that left the page', (t) => {
    const doc = withDocument(t);
    const opener = el(doc, 'opener');
    opener.focus();
    const a = el(doc, 'a');

    focusTrap.activate(container(doc, [a]), 'm8');
    opener.isConnected = false;
    focusTrap.release('m8');

    assert.equal(doc.activeElement, a);
});
