const observers = new WeakMap();

export function measure(element) {
    const rect = element.getBoundingClientRect();
    return [rect.width, rect.height];
}

export function disconnect(element) {
    const state = observers.get(element);
    if (!state) return;
    state.observer.disconnect();
    clearTimeout(state.timer);
    observers.delete(element);
}

export function observe(element, reference, key, width, height) {
    disconnect(element);
    if (width > 0 && height > 0) {
        element.style.width = Math.min(width, Math.max(1, window.innerWidth - 16)) + "px";
        element.style.height = Math.min(height, Math.max(1, window.innerHeight - 16)) + "px";
        element.style.maxWidth = "calc(100vw - 1rem)";
        element.style.maxHeight = "calc(100vh - 1rem)";
    }
    const rect = element.getBoundingClientRect();
    const state = { width: rect.width, height: rect.height, timer: null, observer: null };
    state.observer = new ResizeObserver(() => {
        const rect = element.getBoundingClientRect();
        if (rect.width === state.width && rect.height === state.height) return;
        state.width = rect.width;
        state.height = rect.height;
        clearTimeout(state.timer);
        state.timer = setTimeout(() => {
            reference.invokeMethodAsync("SizeChanged", key, state.width, state.height)
                .catch(() => { /* The dialog may have closed while the callback was queued. */ });
        }, 150);
    });
    state.observer.observe(element);
    observers.set(element, state);
    return [rect.width, rect.height];
}
