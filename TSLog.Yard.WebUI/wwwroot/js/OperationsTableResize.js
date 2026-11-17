(function initOperationsTableResize() {
    const table = document.querySelector('table.operations-table');
    const options = document.getElementById('operation-column-options');
    if (!table || !table.tHead || !options) return;
    const headers = Array.from(table.tHead.rows[0].cells);
    const visibilityKey = 'tslog-yard-hidden-columns';
    const storageKey = 'tslog-yard-operations-column-widths-by-key';
    const legacyKey = 'tslog-yard-operations-column-widths';
    const legacyOrder = [
        'id', 'type', 'orders', 'client', 'driver', 'phone', 'state', 'arrival', 'workStart',
        'workFinish', 'departure', 'onSiteDuration', 'workDuration', 'vehicle', 'trailer',
        'gate', 'vehicleKind', 'note'
    ];
    const minWidth = 90;
    const maxWidth = 480;
    const clamp = width => Math.min(maxWidth, Math.max(minWidth, width));
    const initialWidths = headers.map(th => clamp(Math.round(th.getBoundingClientRect().width)));
    const widths = initialWidths.slice();
    const hidden = new Set(headers.filter(th => th.hasAttribute('data-hidden-by-default')).map(th => th.dataset.column));
    try {
        const saved = JSON.parse(localStorage.getItem(storageKey));
        const legacy = JSON.parse(localStorage.getItem(legacyKey));
        headers.forEach((th, index) => {
            const key = th.dataset.column;
            const legacyWidth = legacy?.length === legacyOrder.length ? legacy[legacyOrder.indexOf(key)] : undefined;
            const width = saved?.[key] ?? legacyWidth;
            if (Number.isFinite(width)) widths[index] = clamp(width);
        });
    } catch (_) { }
    try {
        const saved = JSON.parse(localStorage.getItem(visibilityKey));
        if (Array.isArray(saved)) {
            hidden.clear();
            saved.forEach(key => hidden.add(key));
        }
    } catch (_) { }

    const viewport = document.createElement('div');
    viewport.className = 'yard-table-scroll';
    viewport.tabIndex = 0;
    viewport.setAttribute('role', 'region');
    viewport.setAttribute('aria-label', 'Таблица заявок — горизонтальная прокрутка');
    table.before(viewport);
    viewport.append(table);
    const colgroup = document.createElement('colgroup');
    const columns = headers.map(() => document.createElement('col'));
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
        const byColumn = Object.fromEntries(headers.map((th, index) => [th.dataset.column, widths[index]]));
        try { localStorage.setItem(storageKey, JSON.stringify(byColumn)); } catch (_) { }
    }

    function render() {
        let tableWidth = 2;
        headers.forEach((th, index) => {
            const col = columns[index];
            col.style.width = widths[index] + 'px';
            if (!th.hidden) tableWidth += widths[index];
            const handle = th.querySelector('.yard-column-resize');
            if (handle) handle.setAttribute('aria-valuenow', String(Math.round(widths[index])));
        });
        table.style.width = tableWidth + 'px';
        scheduleLayout();
    }

    function renderVisibility() {
        const visibleColumns = [];
        headers.forEach((th, index) => {
            th.hidden = !th.hasAttribute('data-always-visible') && hidden.has(th.dataset.column);
            if (!th.hidden) visibleColumns.push(columns[index]);
        });
        colgroup.replaceChildren(...visibleColumns);

        const hiddenHeaders = new Set(headers.filter(th => th.hidden).map(th => th.dataset.column));
        for (const cell of table.tBodies[0].querySelectorAll('td[data-column]')) {
            cell.hidden = hiddenHeaders.has(cell.dataset.column);
        }
        render();
    }

    headers.forEach((th, index) => {
        const columnName = th.innerText.replace(/\s+/g, ' ').trim();
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        const alwaysVisible = th.hasAttribute('data-always-visible');
        checkbox.checked = alwaysVisible || !hidden.has(th.dataset.column);
        checkbox.disabled = alwaysVisible;
        checkbox.addEventListener('change', () => {
            if (checkbox.checked) hidden.delete(th.dataset.column);
            else hidden.add(th.dataset.column);
            try { localStorage.setItem(visibilityKey, JSON.stringify([...hidden])); } catch (_) { }
            renderVisibility();
        });
        const label = document.createElement('label');
        label.append(checkbox, columnName);
        options.append(label);

        const handle = document.createElement('span');
        handle.className = 'yard-column-resize';
        handle.tabIndex = 0;
        handle.setAttribute('role', 'separator');
        handle.setAttribute('aria-orientation', 'vertical');
        handle.setAttribute('aria-label', 'Ширина столбца «' + columnName + '»');
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
    new ResizeObserver(scheduleLayout).observe(table);
    renderVisibility();
})();
