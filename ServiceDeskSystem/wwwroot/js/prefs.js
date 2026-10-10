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

    const api = { SCHEMA, OBSOLETE_KEYS, normalize, migrate };

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = api;
    }
    if (typeof window !== 'undefined') {
        window.prefs = api;
    }
})();
