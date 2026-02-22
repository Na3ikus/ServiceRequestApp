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
    theme: {
        extend: {},
    },
    plugins: [],
}