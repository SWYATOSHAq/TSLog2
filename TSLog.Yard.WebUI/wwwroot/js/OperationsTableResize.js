(function initOperationsTableResize() {
    const table = document.querySelector('table.operations-table');
    if (!table || !table.tHead) return;
    const headers = Array.from(table.tHead.rows[0].cells);
    const storageKey = 'tslog-yard-operations-column-widths';
    const minWidth = 90;
    const maxWidth = 480;
    const clamp = width => Math.min(maxWidth, Math.max(minWidth, width));
    const initialWidths = headers.map(th => clamp(Math.round(th.getBoundingClientRect().width)));
    let widths = initialWidths.slice();
    try {
        const saved = JSON.parse(localStorage.getItem(storageKey));
        if (Array.isArray(saved) && saved.length === headers.length && saved.every(Number.isFinite)) {
            widths = saved.map(clamp);
        }
    } catch (_) { /* Resizing works even if storage is unavailable. */ }

    const viewport = document.createElement('div');
    viewport.className = 'yard-table-scroll';
    viewport.tabIndex = 0;
    viewport.setAttribute('role', 'region');
    viewport.setAttribute('aria-label', 'Таблица заявок — горизонтальная прокрутка');
    table.before(viewport);
    viewport.append(table);
    const colgroup = document.createElement('colgroup');
    const columns = headers.map(() => colgroup.appendChild(document.createElement('col')));
    table.prepend(colgroup);
    table.classList.add('yard-resizable-table');

    // A native horizontal scrollbar stays within reach while the table is visible.
    const scrollbar = document.createElement('div');
    scrollbar.className = 'yard-table-scrollbar hide-on-print';
    scrollbar.tabIndex = 0;
    scrollbar.setAttribute('role', 'region');
    scrollbar.setAttribute('aria-label', 'Прокрутить таблицу заявок влево или вправо');
    scrollbar.hidden = true;
    const scrollContent = scrollbar.appendChild(document.createElement('div'));
    document.body.append(scrollbar);
    let frame = 0;
    function scheduleLayout() {
        if (!frame) frame = requestAnimationFrame(updateLayout);
    }
    function updateLayout() {
        frame = 0;
        const bounds = viewport.getBoundingClientRect();
        const tableBounds = table.getBoundingClientRect();
        const height = 24;
        const left = Math.max(0, bounds.left);
        const right = Math.min(document.documentElement.clientWidth, bounds.left + viewport.clientWidth);
        const top = Math.min(window.innerHeight - height, bounds.bottom - height);
        scrollbar.hidden = viewport.scrollWidth <= viewport.clientWidth + 1 || right <= left
            || top <= Math.max(0, bounds.top);
        scrollbar.style.left = left + 'px';
        scrollbar.style.top = top + 'px';
        scrollbar.style.width = Math.max(0, right - left) + 'px';
        // Match the scroll range even when the viewport is partially clipped.
        scrollContent.style.width = (viewport.scrollWidth - viewport.clientWidth + scrollbar.clientWidth) + 'px';
        scrollbar.scrollLeft = viewport.scrollLeft;
        headers.forEach(th => {
            const handle = th.querySelector('.yard-column-resize');
            if (handle) handle.style.height = Math.max(0, tableBounds.bottom - th.getBoundingClientRect().top - 2) + 'px';
        });
    }
    scrollbar.addEventListener('scroll', () => {
        if (Math.abs(viewport.scrollLeft - scrollbar.scrollLeft) > 1) viewport.scrollLeft = scrollbar.scrollLeft;
    }, { passive: true });
    viewport.addEventListener('scroll', () => {
        if (Math.abs(scrollbar.scrollLeft - viewport.scrollLeft) > 1) scrollbar.scrollLeft = viewport.scrollLeft;
    }, { passive: true });
    window.addEventListener('scroll', scheduleLayout, { passive: true, capture: true });
    window.addEventListener('resize', scheduleLayout, { passive: true });
    // Body rows are replaced after filtering; the handles stay on the headers
    // and extend down the entire column without adding listeners to every cell.
    const observer = new ResizeObserver(scheduleLayout);
    observer.observe(table);
    observer.observe(viewport);

    function save() {
        try { localStorage.setItem(storageKey, JSON.stringify(widths)); } catch (_) { }
    }
    function render() {
        columns.forEach((col, index) => { col.style.width = widths[index] + 'px'; });
        table.style.width = (widths.reduce((sum, width) => sum + width, 0) + 2) + 'px';
        headers.forEach((th, index) => {
            const handle = th.querySelector('.yard-column-resize');
            if (handle) handle.setAttribute('aria-valuenow', String(Math.round(widths[index])));
        });
        scheduleLayout();
    }
    headers.forEach((th, index) => {
        const handle = document.createElement('span');
        handle.className = 'yard-column-resize';
        handle.tabIndex = 0;
        handle.setAttribute('role', 'separator');
        handle.setAttribute('aria-orientation', 'vertical');
        handle.setAttribute('aria-label', 'Ширина столбца «' + th.textContent.trim() + '»');
        handle.setAttribute('aria-valuemin', String(minWidth));
        handle.setAttribute('aria-valuemax', String(maxWidth));
        handle.title = 'Потяните границу столбца; двойной щелчок — исходная ширина';
        th.append(handle);
        let drag = null;
        handle.addEventListener('pointerdown', event => {
            if (event.button !== 0) return;
            event.preventDefault();
            event.stopPropagation();
            handle.focus({ preventScroll: true });
            drag = { x: event.clientX, width: widths[index] };
            handle.setPointerCapture(event.pointerId);
            viewport.classList.add('yard-table-resizing');
        });
        handle.addEventListener('pointermove', event => {
            if (!drag) return;
            widths[index] = clamp(drag.width + event.clientX - drag.x);
            render();
        });
        function finish() {
            if (!drag) return;
            drag = null;
            viewport.classList.remove('yard-table-resizing');
            save();
        }
        handle.addEventListener('pointerup', finish);
        handle.addEventListener('pointercancel', finish);
        handle.addEventListener('lostpointercapture', finish);
        handle.addEventListener('click', event => event.stopPropagation());
        handle.addEventListener('dblclick', event => {
            event.preventDefault();
            event.stopPropagation();
            widths[index] = initialWidths[index];
            render();
            save();
        });
        handle.addEventListener('keydown', event => {
            if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
            event.preventDefault();
            widths[index] = event.key === 'Home' ? minWidth : event.key === 'End' ? maxWidth
                : clamp(widths[index] + (event.key === 'ArrowRight' ? 10 : -10));
            render();
            save();
        });
    });
    render();
})();
