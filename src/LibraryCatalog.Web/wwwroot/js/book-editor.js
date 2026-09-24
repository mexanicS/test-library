(() => {
    const form = document.getElementById('book-form');
    const container = document.getElementById('chapters');
    const template = document.getElementById('chapter-template');
    const addButton = document.getElementById('add-chapter');
    const fileInput = document.getElementById('ContentsFile');
    if (!form || !container || !template || !addButton) return;

    function initializeEditor(card) {
        const element = card.querySelector('.chapter-editor');
        const hidden = card.querySelector('.chapter-html');
        if (!element || !hidden || typeof Quill === 'undefined') return;

        const quill = new Quill(element, {
            theme: 'snow',
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline'],
                    [{ list: 'ordered' }, { list: 'bullet' }],
                    ['clean']
                ]
            }
        });
        if (hidden.value) quill.clipboard.dangerouslyPasteHTML(hidden.value, 'silent');
        card.quillEditor = quill;
    }

    function renumber() {
        const cards = [...container.querySelectorAll('[data-chapter]')];
        cards.forEach((card, index) => {
            card.querySelector('.chapter-label').textContent = `Глава ${index + 1}`;
            card.querySelector('.chapter-title').name = `Chapters[${index}].Title`;
            card.querySelector('.chapter-html').name = `Chapters[${index}].BodyHtml`;
        });
        addButton.disabled = cards.length >= 100;
    }

    function synchronizeFileMode() {
        const importing = (fileInput?.files?.length ?? 0) > 0;
        container.querySelectorAll('.chapter-title').forEach(input => {
            input.required = !importing;
        });
    }

    container.querySelectorAll('[data-chapter]').forEach(initializeEditor);
    renumber();

    addButton.addEventListener('click', () => {
        if (container.querySelectorAll('[data-chapter]').length >= 100) return;
        const fragment = template.content.cloneNode(true);
        const card = fragment.querySelector('[data-chapter]');
        container.appendChild(fragment);
        initializeEditor(card);
        renumber();
        synchronizeFileMode();
        card.querySelector('.chapter-title').focus();
    });

    container.addEventListener('click', event => {
        if (!event.target.closest('[data-remove-chapter]')) return;
        event.target.closest('[data-chapter]').remove();
        renumber();
    });

    fileInput?.addEventListener('change', synchronizeFileMode);

    form.addEventListener('submit', () => {
        container.querySelectorAll('[data-chapter]').forEach(card => {
            if (card.quillEditor) {
                card.querySelector('.chapter-html').value = card.quillEditor.getSemanticHTML();
            }
        });
    });
})();
