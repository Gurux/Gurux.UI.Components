const observers = new WeakMap();
const dialogs = new Map();

export function openDialog(element, key) {
    const root = element?.closest('[role="dialog"]');
    if (!root || dialogs.has(key)) return;
    const previous = document.activeElement;
    const topmost = () => [...dialogs.keys()].at(-1) === key;
    const focusable = () => [...root.querySelectorAll('button, [href], input, select, textarea, [tabindex]')]
        .filter(node => !node.disabled && node.tabIndex >= 0 && node.getClientRects().length);
    const focusFirst = () => (focusable()[0] ?? root).focus();
    const onKeyDown = event => {
        if (event.key !== 'Tab' || !topmost()) return;
        const items = focusable();
        const first = items[0];
        const last = items.at(-1);
        if (!first) { event.preventDefault(); root.focus(); }
        else if (event.shiftKey && (document.activeElement === first || document.activeElement === root)) {
            event.preventDefault(); last.focus();
        } else if (!event.shiftKey && (document.activeElement === last || document.activeElement === root)) {
            event.preventDefault(); first.focus();
        }
    };
    const onFocus = event => {
        if (topmost() && !root.contains(event.target)) focusFirst();
    };
    root.addEventListener('keydown', onKeyDown);
    document.addEventListener('focusin', onFocus);
    dialogs.set(key, { root, previous, onKeyDown, onFocus });
    focusFirst();
}

export function closeDialog(key) {
    const state = dialogs.get(key);
    if (!state) return;
    const wasTopmost = [...dialogs.keys()].at(-1) === key;
    dialogs.delete(key);
    state.root.removeEventListener('keydown', state.onKeyDown);
    document.removeEventListener('focusin', state.onFocus);
    if (wasTopmost && state.previous?.isConnected) state.previous.focus();
}

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
