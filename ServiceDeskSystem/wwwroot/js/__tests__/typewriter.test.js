const test = require('node:test');
const assert = require('node:assert/strict');
const typewriter = require('../typewriter.js');

function run(phrases, steps) {
    let state = typewriter.initial();
    const frames = [];
    for (let i = 0; i < steps; i++) {
        state = typewriter.next(state, phrases);
        frames.push(state);
    }
    return frames;
}

test('types a phrase one character at a time', () => {
    const frames = run(['ab', 'c'], 2);
    assert.deepEqual(frames.map((f) => f.text), ['a', 'ab']);
    assert.ok(frames.every((f) => f.delay === typewriter.TYPE_MS));
});

test('holds a typed phrase, erases it and moves to the next', () => {
    const frames = run(['ab', 'c'], 7);
    assert.deepEqual(frames.map((f) => f.text), ['a', 'ab', 'ab', 'a', '', '', 'c']);
    assert.equal(frames[2].delay, typewriter.HOLD_MS);
    assert.equal(frames[3].delay, typewriter.ERASE_MS);
    assert.equal(frames[5].delay, typewriter.GAP_MS);
});

test('wraps around after the last phrase', () => {
    const frames = run(['a', 'b'], 9);
    assert.equal(frames.at(-1).text, 'a');
});

test('keeps the first phrase first and shuffles the rest', () => {
    const phrases = ['tagline', 'q1', 'q2', 'q3'];
    const reversed = typewriter.order(phrases, () => 0);

    assert.equal(reversed[0], 'tagline');
    assert.deepEqual([...reversed].sort(), [...phrases].sort());
    assert.deepEqual(phrases, ['tagline', 'q1', 'q2', 'q3'], 'input is not mutated');
});
