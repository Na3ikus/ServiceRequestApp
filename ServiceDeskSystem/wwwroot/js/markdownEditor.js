window.markdownEditor = {
    insertFormatting: function (element, prefix, suffix) {
        if (!element) return;
        
        const start = element.selectionStart;
        const end = element.selectionEnd;
        const text = element.value;
        
        // If text is selected, wrap it
        if (start !== end) {
            const selectedText = text.substring(start, end);
            const newText = text.substring(0, start) + prefix + selectedText + suffix + text.substring(end);
            element.value = newText;
            element.selectionStart = start + prefix.length;
            element.selectionEnd = end + prefix.length;
        } else {
            // If no text selected, insert the markdown symbols and place cursor between them
            const newText = text.substring(0, start) + prefix + "текст" + suffix + text.substring(end);
            element.value = newText;
            element.selectionStart = start + prefix.length;
            element.selectionEnd = start + prefix.length + 5; // "текст".length
        }
        
        // Dispatch input event so Blazor updates its binding
        const event = new Event('input', { bubbles: true });
        element.dispatchEvent(event);
        
        element.focus();
    }
};
