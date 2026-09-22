const initialized = new WeakMap();

function findTarget(root, hash) {
    if (!hash || hash === '#') return null;
    let id;
    try { id = decodeURIComponent(hash.slice(1)); }
    catch { return null; }
    return [...root.querySelectorAll('[id]')].find(element => element.id === id);
}

export function initialize(root, content) {
    if (!initialized.has(root)) {
        root.addEventListener('click', event => {
            if (event.defaultPrevented || event.button !== 0 || event.ctrlKey ||
                event.metaKey || event.shiftKey || event.altKey) return;
            const link = event.target.closest('a[href]');
            if (!link || !root.contains(link) || link.hasAttribute('download') ||
                (link.target && link.target !== '_self')) return;
            const destination = new URL(link.href);
            if (destination.origin !== location.origin || destination.pathname !== location.pathname ||
                destination.search !== location.search) return;
            const hash = destination.hash;
            const target = findTarget(root, hash);
            if (!target) return;
            // Keep the current Blazor route even when <base href="/"> is present.
            event.preventDefault();
            const url = new URL(location.href);
            url.hash = hash;
            if (location.href !== url.href) history.pushState(history.state, '', url);
            target.scrollIntoView();
            if (!target.hasAttribute('tabindex')) target.setAttribute('tabindex', '-1');
            target.focus({ preventScroll: true });
        });
    }
    // Native modified clicks and copying the link must preserve the current route too.
    for (const link of root.querySelectorAll('a[href^="#"]')) {
        const url = new URL(location.href);
        url.hash = link.getAttribute('href');
        link.href = url.href;
    }
    if (initialized.get(root) !== content) {
        initialized.set(root, content);
        // Source may finish loading after the browser's initial fragment navigation.
        findTarget(root, location.hash)?.scrollIntoView();
    }
}
