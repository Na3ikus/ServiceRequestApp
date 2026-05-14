window.quillInterop = {
    instances: {},

    init: function (elementId, dotNetRef, initialContent, placeholder) {
        var toolbarOptions = [
            ['bold', 'italic', 'underline', 'strike'],
            ['blockquote', 'code-block'],
            [{ 'list': 'ordered'}, { 'list': 'bullet' }],
            [{ 'header': [1, 2, 3, false] }],
            ['link'],
            ['clean']
        ];

        var quill = new Quill('#' + elementId, {
            theme: 'snow',
            placeholder: placeholder || '',
            modules: {
                toolbar: toolbarOptions
            }
        });
        
        if (initialContent) {
            quill.root.innerHTML = initialContent;
        }

        quill.on('text-change', function () {
            dotNetRef.invokeMethodAsync('UpdateContent', quill.root.innerHTML);
        });

        // Prevent Blazor from intercepting clicks on Quill's tooltip links (e.g. the "Save" link)
        var tooltip = quill.theme.tooltip.root;
        if (tooltip) {
            tooltip.addEventListener('click', function(e) {
                if (e.target.closest('a')) {
                    // Let Quill handle it, but prevent Blazor's document-level listener from seeing it
                    e.stopPropagation();
                }
            }, true); // use capture phase to guarantee we catch it early
        }
        
        window.quillInterop.instances[elementId] = quill;
    },
    
    setContent: function(elementId, content) {
        var quill = window.quillInterop.instances[elementId];
        if (quill) {
            quill.root.innerHTML = content || '';
        }
    },

    dispose: function(elementId) {
        delete window.quillInterop.instances[elementId];
    }
};
