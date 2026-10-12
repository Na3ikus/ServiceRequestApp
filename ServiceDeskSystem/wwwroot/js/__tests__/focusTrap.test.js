const test = require('node:test');
const assert = require('node:assert/strict');
const focusTrap = require('../focusTrap.js');

// Minimal DOM stand-ins: what the trap touches and nothing else.
function fakeDoc() {
    return { activeElement: null };
}

function el(doc, name, { visible = true } = {}) {
    return {
        name,
        isConnected: true,
        getClientRects: () => (visible ? [{}] : []),
        focus() { doc.activeElement = this; },
    };
}

function container(doc, items) {
    const listeners = new Map();
    const box = el(doc, 'box');
    box.selectors = [];
    box.querySelectorAll = (selector) => { box.selectors.push(selector); return items; };
    box.addEventListener = (type, fn) => listeners.set(type, fn);
    box.removeEventListener = (type, fn) => { if (listeners.get(type) === fn) listeners.delete(type); };
    box.dispatch = (type, event) => listeners.get(type)?.(event);
    box.listeners = listeners;
    return box;
}

function tab(shiftKey = false) {
    return { key: 'Tab', shiftKey, prevented: false, preventDefault() { this.prevented = true; } };
}

test('lists visible tabbable elements in order', () => {
    const doc = fakeDoc();
    const a = el(doc, 'a');
    const hidden = el(doc, 'hidden', { visible: false });
    const b = el(doc, 'b');
    const box = container(doc, [a, hidden, b]);

    assert.deepEqual(focusTrap.focusables(box).map((x) => x.name), ['a', 'b']);
    assert.equal(box.selectors[0], focusTrap.FOCUSABLE);
    assert.ok(focusTrap.FOCUSABLE.includes('[tabindex]:not([tabindex="-1"])'));
});

test('wraps Tab from the last element to the first and Shift+Tab back', () => {
    const doc = fakeDoc();
    const [a, b, c] = ['a', 'b', 'c'].map((n) => el(doc, n));
    const items = [a, b, c];

    assert.equal(focusTrap.wrapTarget(items, c, false), a);
    assert.equal(focusTrap.wrapTarget(items, a, true), c);
});

test('leaves moves between inner elements to the browser', () => {
    const doc = fakeDoc();
    const [a, b, c] = ['a', 'b', 'c'].map((n) => el(doc, n));

    assert.equal(focusTrap.wrapTarget([a, b, c], a, false), null);
    assert.equal(focusTrap.wrapTarget([a, b, c], c, true), null);
});

test('pulls focus back in from outside (e.g. from the dialog itself)', () => {
    const doc = fakeDoc();
    const [a, b] = ['a', 'b'].map((n) => el(doc, n));
    const outside = el(doc, 'outside');

    assert.equal(focusTrap.wrapTarget([a, b], outside, false), a);
    assert.equal(focusTrap.wrapTarget([a, b], outside, true), b);
});

test('keeps focus on the container when nothing inside is tabbable', () => {
    const doc = fakeDoc();
    const box = container(doc, []);

    assert.equal(focusTrap.wrapTarget([], null, false, box), box);
});

test('Tab on the last element is cancelled and focus goes to the first', () => {
    const doc = fakeDoc();
    const [a, b] = ['a', 'b'].map((n) => el(doc, n));
    const box = container(doc, [a, b]);
    b.focus();

    const event = tab();
    focusTrap.onKeyDown(box, event, doc);

    assert.equal(event.prevented, true);
    assert.equal(doc.activeElement, a);
});

test('other keys and inner Tab moves are not touched', () => {
    const doc = fakeDoc();
    const [a, b] = ['a', 'b'].map((n) => el(doc, n));
    const box = container(doc, [a, b]);
    a.focus();

    const event = tab();
    focusTrap.onKeyDown(box, event, doc);
    const enter = { key: 'Enter', prevented: false, preventDefault() { this.prevented = true; } };
    focusTrap.onKeyDown(box, enter, doc);

    assert.equal(event.prevented, false);
    assert.equal(enter.prevented, false);
    assert.equal(doc.activeElement, a);
});

test('activate focuses the first element, release restores the opener', (t) => {
    const doc = fakeDoc();
    t.after(() => { delete globalThis.document; });
    globalThis.document = doc;

    const opener = el(doc, 'opener');
    opener.focus();
    const [a, b] = ['a', 'b'].map((n) => el(doc, n));
    const box = container(doc, [a, b]);

    focusTrap.activate(box, 'm1');
    assert.equal(doc.activeElement, a);
    assert.ok(box.listeners.has('keydown'));

    b.focus();
    const event = tab();
    box.dispatch('keydown', event);
    assert.equal(doc.activeElement, a, 'the keydown listener wraps Tab');

    focusTrap.release('m1');
    assert.equal(doc.activeElement, opener);
    assert.equal(box.listeners.has('keydown'), false);

    focusTrap.release('m1'); // second release is a no-op
    assert.equal(doc.activeElement, opener);
});

test('activate focuses the dialog itself when it has nothing tabbable', (t) => {
    const doc = fakeDoc();
    t.after(() => { delete globalThis.document; });
    globalThis.document = doc;
    const box = container(doc, []);

    focusTrap.activate(box, 'm2');
    assert.equal(doc.activeElement, box);
    focusTrap.release('m2');
});

test('release skips an opener that left the page', (t) => {
    const doc = fakeDoc();
    t.after(() => { delete globalThis.document; });
    globalThis.document = doc;

    const opener = el(doc, 'opener');
    opener.focus();
    const a = el(doc, 'a');
    const box = container(doc, [a]);

    focusTrap.activate(box, 'm3');
    opener.isConnected = false;
    focusTrap.release('m3');

    assert.equal(doc.activeElement, a);
});
