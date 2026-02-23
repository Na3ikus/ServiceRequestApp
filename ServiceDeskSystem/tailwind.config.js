/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Components/**/*.razor",
        "./Components/**/*.cshtml",
        "./Pages/**/*.razor",
        "./Pages/**/*.cshtml",
        "./**/*.razor",
        "./**/*.html",
        "./**/*.cs"
    ],
    darkMode: 'class',
    safelist: [
        'hover:bg-blue-100',
        'hover:bg-blue-50/50'
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}