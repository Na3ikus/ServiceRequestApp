/** @type {import('tailwindcss').Config} */
const plugin = require('tailwindcss/plugin');
const token = (name) => `rgb(var(--${name}) / <alpha-value>)`;

module.exports = {
    content: [
        "./Components/**/*.razor",
        "./Components/**/*.cshtml",
        "./Components/**/*.cs",
        "./Pages/**/*.razor",
        "./Pages/**/*.cshtml",
        "./wwwroot/**/*.html"
    ],
    darkMode: 'class',
    safelist: [
        'hover:bg-blue-100',
        'hover:bg-blue-50/50'
    ],
    theme: {
        extend: {
            fontFamily: {
                sans: ['Geist', 'Inter', 'system-ui', 'sans-serif'],
                mono: ['"Geist Mono"', '"JetBrains Mono"', 'ui-monospace', 'monospace'],
            },
            colors: {
                canvas: token('canvas'),
                surface: token('surface'),
                line: token('line'),
                fg: token('fg'),
                muted: token('muted'),
                accent: token('accent'),
                'accent-ink': token('accent-ink'),
                'on-accent': token('on-accent'),
                danger: token('danger'),
            },
            // Type scale in rem so the interface-scale setting (html font-size %) resizes text.
            fontSize: {
                '12': '0.75rem',
                '13': '0.8125rem',
                '14': '0.875rem',
                '15': '0.9375rem',
                '20': '1.25rem',
                '24': '1.5rem',
                '40': '2.5rem',
            },
            // Accent tints in light theme: hover 7 %, open / editing rows 10 %, active nav item 22 %.
            opacity: {
                '7': '0.07',
                '22': '0.22',
            },
            animation: {
                'fade-in': 'fadeIn 0.3s ease-in-out',
                'slide-up': 'slideUp 0.4s ease-out',
                'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
            },
            keyframes: {
                fadeIn: {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                slideUp: {
                    '0%': { transform: 'translateY(10px)', opacity: '0' },
                    '100%': { transform: 'translateY(0)', opacity: '1' },
                }
            }
        },
    },
    plugins: [
        // light: — the counterpart of dark: for light-only styles (accent tints).
        plugin(({ addVariant }) => addVariant('light', 'html:not(.dark) &')),
    ],
}
