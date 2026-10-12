window.commandPalette = {
    _dotNetRef: null,

    register(dotNetRef) {
        this._dotNetRef = dotNetRef;

        document.addEventListener('keydown', (e) => {
            // Ctrl+K or Cmd+K
            if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'K')) {
                e.preventDefault();
                e.stopPropagation();
                this._dotNetRef?.invokeMethodAsync('Toggle');
            }
        });
    },

    // Keeps the keyboard-selected option visible in the scrolling list.
    scrollToSelected(list) {
        list?.querySelector('[aria-selected="true"]')?.scrollIntoView({ block: 'nearest' });
    }
};
