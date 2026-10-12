const test = require('node:test');
const assert = require('node:assert/strict');
const palette = require('../commandPalette.js');

function setup() {
    const calls = [];
    palette._dotNetRef = { invokeMethodAsync: (name, ...args) => calls.push([name, ...args]) };
    palette._x = null;
    palette._y = null;
    palette._index = -1;
    palette._keyboard = false;
    return calls;
}

function row(index) {
    const el = { getAttribute: (n) => (n === 'data-cmd-index' ? String(index) : null) };
    return { closest: (sel) => (sel === '[data-cmd-index]' ? el : null) };
}

const outside = { closest: () => null };

function move(target, x, y) {
    palette.onPointerMove({ target, clientX: x, clientY: y });
}

test('pointer move selects the row under the pointer', () => {
    const calls = setup();
    move(row(3), 10, 10);
    assert.deepEqual(calls, [['HoverItem', 3]]);
});

test('moving within the same row calls .NET once', () => {
    const calls = setup();
    move(row(3), 10, 10);
    move(row(3), 12, 11);
    assert.equal(calls.length, 1);
});

test('events with unchanged coordinates are ignored', () => {
    const calls = setup();
    move(row(3), 10, 10);
    palette.onKeyDown();
    // List scrolled under a still cursor: another row is under the pointer now.
    move(row(5), 10, 10);
    assert.deepEqual(calls, [['HoverItem', 3]]);
});

test('a real move after keyboard use reselects the row under the pointer', () => {
    const calls = setup();
    move(row(3), 10, 10);
    palette.onKeyDown();
    move(row(3), 11, 10);
    assert.deepEqual(calls, [['HoverItem', 3], ['HoverItem', 3]]);
});

test('moves outside the rows are ignored', () => {
    const calls = setup();
    move(outside, 10, 10);
    move({}, 20, 20);
    assert.deepEqual(calls, []);
});
