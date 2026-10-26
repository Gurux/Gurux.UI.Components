// Delegate events so columns added or shown after initialization work as well.
export function initialize(id, component) {
    const table = document.getElementById(id);
    const listeners = new AbortController();
    const options = { signal: listeners.signal };
    let drag = null;
    let suppressClick = false;

    function headerAt(event) {
        const header = event.target.closest?.("th[data-gx-column]");
        return header?.closest("table") === table && header.dataset.gxColumn ? header : null;
    }

    function atEdge(header, event) {
        return Math.abs(header.getBoundingClientRect().right - event.clientX) <= 6;
    }

    function applyWidth(header, width) {
        header.style.width = `${width}px`;
        header.style.minWidth = `${width}px`;
        header.style.maxWidth = `${width}px`;
    }

    table.addEventListener("pointermove", event => {
        const header = headerAt(event);
        if (header) header.style.cursor = atEdge(header, event) ? "col-resize" : "";
    }, options);

    table.addEventListener("pointerdown", event => {
        const header = headerAt(event);
        if (event.button !== 0 || !header || !atEdge(header, event)) return;
        event.preventDefault();
        event.stopPropagation();
        const rect = header.getBoundingClientRect();
        const style = getComputedStyle(header);
        const extra = style.boxSizing === "border-box" ? 0 :
            parseFloat(style.paddingLeft) + parseFloat(style.paddingRight) +
            parseFloat(style.borderLeftWidth) + parseFloat(style.borderRightWidth);
        drag = { header, start: event.clientX, width: rect.width - extra, pointer: event.pointerId };
        header.setPointerCapture(event.pointerId);
    }, { ...options, capture: true });

    table.addEventListener("pointermove", event => {
        if (!drag || drag.pointer !== event.pointerId) return;
        event.preventDefault();
        applyWidth(drag.header, Math.max(24, drag.width + event.clientX - drag.start));
    }, options);

    function finish(event) {
        if (!drag || drag.pointer !== event.pointerId) return;
        const { header, width, start, pointer } = drag;
        drag = null;
        if (header.hasPointerCapture(pointer)) header.releasePointerCapture(pointer);
        suppressClick = true;
        setTimeout(() => { suppressClick = false; }, 0);
        if (event.type === "pointercancel") {
            applyWidth(header, width);
            return;
        }
        const savedWidth = Math.max(24, width + event.clientX - start);
        applyWidth(header, savedWidth);
        component.invokeMethodAsync("SaveColumnWidthAsync", header.dataset.gxColumn, savedWidth)
            .catch(error => console.error("Unable to save table column width.", error));
    }
    table.addEventListener("pointerup", finish, options);
    table.addEventListener("pointercancel", finish, options);
    table.addEventListener("click", event => {
        if (suppressClick) { event.preventDefault(); event.stopPropagation(); }
    }, { ...options, capture: true });

    return { dispose() { listeners.abort(); drag = null; } };
}
