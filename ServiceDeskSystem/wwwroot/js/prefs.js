// User preferences: schema, validation, legacy migration and (in the browser)
// applying them to <html> before first paint. Single source of truth for every
// preference stored in localStorage.
(function () {
    'use strict';

    const BOOL = ['true', 'false'];

    const SCHEMA = {
        theme: { key: 'theme', def: 'system', values: ['light', 'dark', 'system'] },
        accent: { key: 'settings.accentColor', def: 'lime', values: ['lime', 'amber', 'phosphor', 'ice'] },
        scale: { key: 'settings.fontSize', def: '100', values: ['90', '100', '110', '120'] },
        contrast: { key: 'settings.contrastMode', def: 'standard', values: ['standard', 'high'] },
        reducedMotion: { key: 'settings.reducedMotion', def: 'false', values: BOOL },
        density: { key: 'settings.tableDensity', def: 'comfortable', values: ['comfortable', 'compact'] },
        contentWidth: { key: 'settings.contentWidth', def: 'standard', values: ['narrow', 'standard', 'wide'] },
        dateFormat: { key: 'settings.dateFormat', def: 'DD/MM/YYYY', values: ['DD/MM/YYYY', 'MM/DD/YYYY', 'YYYY-MM-DD'] },
        defaultView: { key: 'settings.defaultView', def: 'TABLE', values: ['TABLE', 'KANBAN'] },
        sound: { key: 'settings.soundEnabled', def: 'true', values: BOOL },
        desktopNotifications: { key: 'settings.desktopNotifications', def: 'false', values: BOOL },
        toastPosition: { key: 'settings.notifPosition', def: 'top-right', values: ['top-right', 'top-center', 'bottom-right', 'bottom-center'] },
        shortcuts: { key: 'settings.keyboardShortcuts', def: 'true', values: BOOL },
    };

    const OBSOLETE_KEYS = [
        'settings.cardStyle',
        'settings.hoverStyle',
        'settings.bgTexture',
        'settings.surfaceStyle',
        'settings.borderRadius',
        'settings.fontFamily',
        'settings.sidebarColor',
        'settings.pageTransition',
        'settings.badgeStyle',
        'settings.customAccentHex',
        'settings.animations',
    ];

    const MIGRATED_KEY = 'prefs.migrated';

    function entry(name) {
        const e = SCHEMA[name];
        if (!e) throw new Error('Unknown preference: ' + name);
        return e;
    }

    function normalize(name, raw) {
        const e = entry(name);
        return e.values.includes(raw) ? raw : e.def;
    }

    // Nearest allowed scale; ties round up (95 → 100).
    function snapScale(raw) {
        const n = Number(raw);
        if (raw === '' || !Number.isFinite(n)) return null;
        let best = null;
        for (const v of SCHEMA.scale.values) {
            const d = Math.abs(Number(v) - n);
            if (best === null || d <= best.d) best = { v, d };
        }
        return best.v;
    }

    function migrate(storage) {
        if (storage.getItem(MIGRATED_KEY) === '1') return;

        const accent = storage.getItem(SCHEMA.accent.key);
        if (accent !== null && !SCHEMA.accent.values.includes(accent)) {
            storage.setItem(SCHEMA.accent.key, SCHEMA.accent.def);
        }

        if (storage.getItem('settings.animations') === 'false') {
            storage.setItem(SCHEMA.reducedMotion.key, 'true');
        }

        const scale = storage.getItem(SCHEMA.scale.key);
        if (scale !== null && !SCHEMA.scale.values.includes(scale)) {
            const snapped = snapScale(scale);
            if (snapped === null) storage.removeItem(SCHEMA.scale.key);
            else storage.setItem(SCHEMA.scale.key, snapped);
        }

        for (const key of OBSOLETE_KEYS) storage.removeItem(key);

        storage.setItem(MIGRATED_KEY, '1');
    }

    // ---- Runtime (browser). DOM/storage are looked up at call time so the
    // pure functions above stay loadable under Node.

    const DARK_QUERY = '(prefers-color-scheme: dark)';
    const MOTION_QUERY = '(prefers-reduced-motion: reduce)';
    const THEME_TRANSITION_MS = 220;

    let transitionTimer = null;
    let systemThemeListener = null;

    function root() {
        return globalThis.document.documentElement;
    }

    function read(key) {
        try {
            return globalThis.localStorage.getItem(key);
        } catch (e) {
            return null;
        }
    }

    function osMatches(query) {
        return typeof globalThis.matchMedia === 'function' && globalThis.matchMedia(query).matches;
    }

    function get(name) {
        const e = entry(name);
        const raw = read(e.key);
        if (name === 'reducedMotion' && raw === null) {
            return osMatches(MOTION_QUERY) ? 'true' : 'false';
        }
        return normalize(name, raw);
    }

    function getAll() {
        const all = {};
        for (const name of Object.keys(SCHEMA)) all[name] = get(name);
        return all;
    }

    function resolveTheme() {
        const theme = get('theme');
        if (theme === 'system') return osMatches(DARK_QUERY) ? 'dark' : 'light';
        return theme;
    }

    function apply(name, value) {
        const html = root();
        switch (name) {
            case 'theme':
                html.classList.toggle('dark', resolveTheme() === 'dark');
                break;
            case 'accent':
                html.setAttribute('data-accent', value);
                break;
            case 'scale':
                html.style.fontSize = value + '%';
                break;
            case 'contrast':
                html.setAttribute('data-contrast', value);
                break;
            case 'reducedMotion':
                html.setAttribute('data-motion', value === 'true' ? 'reduced' : 'full');
                break;
            case 'density':
                html.setAttribute('data-density', value);
                break;
            case 'contentWidth':
                html.setAttribute('data-content-width', value);
                break;
            case 'toastPosition':
                html.setAttribute('data-notif-position', value);
                break;
            default:
                break; // no DOM effect
        }
    }

    function applyAll() {
        for (const name of Object.keys(SCHEMA)) apply(name, get(name));
    }

    // Smooth color transition for explicit theme switches only (not on load).
    function startThemeTransition() {
        const html = root();
        if (transitionTimer) clearTimeout(transitionTimer);
        html.classList.add('theme-transitioning');
        transitionTimer = setTimeout(() => {
            html.classList.remove('theme-transitioning');
            transitionTimer = null;
        }, THEME_TRANSITION_MS);
    }

    function set(name, value) {
        const e = entry(name);
        if (!e.values.includes(value)) return false;
        try {
            globalThis.localStorage.setItem(e.key, value);
        } catch (err) {
            // Storage unavailable: still apply for this page view.
        }
        if (name === 'theme') startThemeTransition();
        apply(name, value);
        return true;
    }

    function reset() {
        for (const name of Object.keys(SCHEMA)) {
            try {
                globalThis.localStorage.removeItem(SCHEMA[name].key);
            } catch (e) {
                // Ignore storage errors.
            }
        }
        applyAll();
    }

    function onSystemThemeChange(dotNetRef) {
        systemThemeListener = dotNetRef;
    }

    function init() {
        try {
            migrate(globalThis.localStorage);
        } catch (e) {
            // Storage unavailable: defaults still apply below.
        }
        applyAll();

        if (typeof globalThis.matchMedia !== 'function') return;

        globalThis.matchMedia(DARK_QUERY).addEventListener('change', (ev) => {
            if (get('theme') !== 'system') return;
            apply('theme', 'system');
            if (systemThemeListener) {
                systemThemeListener.invokeMethodAsync('OnSystemThemeChanged', !!ev.matches);
            }
        });
        globalThis.matchMedia(MOTION_QUERY).addEventListener('change', () => {
            if (read(SCHEMA.reducedMotion.key) !== null) return;
            apply('reducedMotion', get('reducedMotion'));
        });
    }

    const api = {
        SCHEMA,
        OBSOLETE_KEYS,
        normalize,
        migrate,
        init,
        get,
        getAll,
        set,
        reset,
        applyAll,
        resolveTheme,
        onSystemThemeChange,
    };

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = api;
    }
    if (typeof window !== 'undefined') {
        window.prefs = api;
        init();
    }
})();
