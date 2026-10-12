const test = require('node:test');
const assert = require('node:assert/strict');
const prefs = require('../prefs.js');

function memoryStorage(initial = {}) {
    const data = new Map(Object.entries(initial));
    return {
        getItem: (k) => (data.has(k) ? data.get(k) : null),
        setItem: (k, v) => data.set(k, String(v)),
        removeItem: (k) => data.delete(k),
        snapshot: () => Object.fromEntries(data),
    };
}

function fakeBrowser({ storage = {}, osDark = false, osReducedMotion = false } = {}) {
    const classes = new Set();
    const attrs = new Map();
    const root = {
        classList: {
            add: (c) => classes.add(c),
            remove: (c) => classes.delete(c),
            toggle: (c, force) => (force ? classes.add(c) : classes.delete(c)),
            contains: (c) => classes.has(c),
        },
        setAttribute: (n, v) => attrs.set(n, String(v)),
        getAttribute: (n) => (attrs.has(n) ? attrs.get(n) : null),
        removeAttribute: (n) => attrs.delete(n),
        style: {},
    };
    const queries = {
        '(prefers-color-scheme: dark)': { matches: osDark, listeners: [] },
        '(prefers-reduced-motion: reduce)': { matches: osReducedMotion, listeners: [] },
    };
    const local = memoryStorage(storage);
    globalThis.document = { documentElement: root };
    globalThis.localStorage = local;
    globalThis.matchMedia = (q) => {
        const mq = queries[q];
        return {
            get matches() { return mq.matches; },
            addEventListener: (_type, fn) => mq.listeners.push(fn),
        };
    };
    return {
        root,
        classes,
        storage: local,
        setOsDark(value) {
            const mq = queries['(prefers-color-scheme: dark)'];
            mq.matches = value;
            mq.listeners.forEach((fn) => fn({ matches: value }));
        },
    };
}

test('set rejects invalid value without writing', () => {
    const b = fakeBrowser();
    prefs.init();
    const before = b.storage.snapshot();
    assert.equal(prefs.set('density', 'huge'), false);
    assert.deepEqual(b.storage.snapshot(), before);
});

test('set applies attribute', () => {
    const b = fakeBrowser();
    prefs.init();
    assert.equal(prefs.set('density', 'compact'), true);
    assert.equal(b.root.getAttribute('data-density'), 'compact');
    assert.equal(b.storage.getItem('settings.tableDensity'), 'compact');
});

test('theme system follows OS', () => {
    const b = fakeBrowser({ storage: { theme: 'system' }, osDark: true });
    prefs.init();
    assert.equal(b.classes.has('dark'), true);
    b.setOsDark(false);
    assert.equal(b.classes.has('dark'), false);
});

test('reset restores defaults', () => {
    const b = fakeBrowser();
    prefs.init();
    prefs.set('accent', 'amber');
    assert.equal(b.root.getAttribute('data-accent'), 'amber');
    prefs.reset();
    assert.equal(prefs.get('accent'), 'lime');
    assert.equal(b.root.getAttribute('data-accent'), 'lime');
    assert.equal(b.storage.getItem('settings.accentColor'), null);
});

test('normalize returns allowed value', () => {
    assert.equal(prefs.normalize('accent', 'amber'), 'amber');
});

test('normalize rejects unknown values', () => {
    assert.equal(prefs.normalize('accent', 'custom'), 'lime');
    assert.equal(prefs.normalize('scale', 'abc'), '100');
    assert.equal(prefs.normalize('accent', '<script>'), 'lime');
    assert.equal(prefs.normalize('accent', null), 'lime');
});

test('normalize throws on unknown name', () => {
    assert.throws(() => prefs.normalize('nope', 'x'), Error);
});

test('migrate maps legacy accent', () => {
    for (const legacy of ['custom', 'blue', 'cyan', 'slate']) {
        const s = memoryStorage({ 'settings.accentColor': legacy });
        prefs.migrate(s);
        assert.equal(s.getItem('settings.accentColor'), 'lime', legacy);
    }
    const kept = memoryStorage({ 'settings.accentColor': 'amber' });
    prefs.migrate(kept);
    assert.equal(kept.getItem('settings.accentColor'), 'amber');
});

test('migrate converts animations=false to reducedMotion=true', () => {
    const s = memoryStorage({ 'settings.animations': 'false' });
    prefs.migrate(s);
    assert.equal(s.getItem('settings.reducedMotion'), 'true');
    assert.equal(s.getItem('settings.animations'), null);

    const on = memoryStorage({ 'settings.animations': 'true' });
    prefs.migrate(on);
    assert.equal(on.getItem('settings.reducedMotion'), null);
    assert.equal(on.getItem('settings.animations'), null);
});

test('migrate snaps fontSize', () => {
    const cases = { '85': '90', '95': '100', '105': '110', '115': '120' };
    for (const [from, to] of Object.entries(cases)) {
        const s = memoryStorage({ 'settings.fontSize': from });
        prefs.migrate(s);
        assert.equal(s.getItem('settings.fontSize'), to, from);
    }
    const bad = memoryStorage({ 'settings.fontSize': 'abc' });
    prefs.migrate(bad);
    assert.equal(bad.getItem('settings.fontSize'), null);
});

test('migrate removes all OBSOLETE_KEYS', () => {
    const initial = {};
    for (const k of prefs.OBSOLETE_KEYS) initial[k] = 'x';
    assert.equal(prefs.OBSOLETE_KEYS.length, 12);
    const s = memoryStorage(initial);
    prefs.migrate(s);
    for (const k of prefs.OBSOLETE_KEYS) assert.equal(s.getItem(k), null, k);
});

test('migrate is idempotent', () => {
    const s = memoryStorage({ 'settings.accentColor': 'custom', 'settings.fontSize': '95' });
    prefs.migrate(s);
    const first = s.snapshot();
    assert.equal(first['prefs.migrated'], '1');
    s.setItem('settings.accentColor', 'custom'); // a later value is not re-migrated
    prefs.migrate(s);
    assert.equal(s.getItem('settings.accentColor'), 'custom');
    assert.deepEqual({ ...s.snapshot(), 'settings.accentColor': 'lime' }, first);
});

test('preview applies accent without storing it', () => {
    const b = fakeBrowser({ storage: { 'settings.accentColor': 'lime' } });
    prefs.init();
    assert.equal(prefs.preview('accent', 'amber'), true);
    assert.equal(b.root.getAttribute('data-accent'), 'amber');
    assert.equal(b.storage.getItem('settings.accentColor'), 'lime');
    assert.equal(prefs.get('accent'), 'lime');
});

test('preview rejects invalid value', () => {
    const b = fakeBrowser();
    prefs.init();
    assert.equal(prefs.preview('accent', 'pink'), false);
    assert.equal(b.root.getAttribute('data-accent'), 'lime');
});

test('preview applies theme without storing it', () => {
    const b = fakeBrowser({ storage: { theme: 'light' } });
    prefs.init();
    prefs.preview('theme', 'dark');
    assert.equal(b.classes.has('dark'), true);
    assert.equal(b.storage.getItem('theme'), 'light');
});

test('restore re-applies the stored value', () => {
    const b = fakeBrowser({ storage: { theme: 'light', 'settings.accentColor': 'ice' } });
    prefs.init();
    prefs.preview('accent', 'amber');
    prefs.preview('theme', 'dark');
    prefs.restore('accent');
    assert.equal(b.root.getAttribute('data-accent'), 'ice');
    assert.equal(b.classes.has('dark'), true);
    prefs.restore('theme');
    assert.equal(b.classes.has('dark'), false);
});

test('restoreAll re-applies every previewed value', () => {
    const b = fakeBrowser({ storage: { theme: 'dark', 'settings.accentColor': 'phosphor' } });
    prefs.init();
    prefs.preview('accent', 'amber');
    prefs.preview('theme', 'light');
    prefs.restoreAll();
    assert.equal(b.root.getAttribute('data-accent'), 'phosphor');
    assert.equal(b.classes.has('dark'), true);
});

test('set after preview stores the value and restore keeps it', () => {
    const b = fakeBrowser();
    prefs.init();
    prefs.preview('accent', 'amber');
    prefs.set('accent', 'ice');
    prefs.restoreAll();
    assert.equal(b.root.getAttribute('data-accent'), 'ice');
    assert.equal(b.storage.getItem('settings.accentColor'), 'ice');
});

test('system theme change does not override a theme preview', () => {
    const b = fakeBrowser({ storage: { theme: 'system' }, osDark: false });
    prefs.init();
    prefs.preview('theme', 'dark');
    b.setOsDark(false);
    assert.equal(b.classes.has('dark'), true);
    prefs.restore('theme');
    assert.equal(b.classes.has('dark'), false);
});
