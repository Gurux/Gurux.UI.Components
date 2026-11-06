const panels = new Map();

function findTarget(root, hash) {
    if (!hash || hash === '#') {
        return null;
    }
    let id;
    try {
        id = decodeURIComponent(hash.replace(/^#/, ''));
    } catch {
        return null;
    }
    return [...root.querySelectorAll('[id]')].find(element => element.id === id);
}

function scrollToTopic(state) {
    if (state.root?.isConnected && state.topic) {
        const target = findTarget(state.root, state.topic);
        if (target) {
            state.topic = null;
            target.scrollIntoView({ block: 'start' });
        }
    }
}

function handleClick(event) {
    if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) {
        return;
    }
    const link = event.target.closest?.('a[href]');
    if (!link || link.hasAttribute('download')) {
        return;
    }
    let destination;
    try {
        destination = new URL(link.href, location.href);
    } catch {
        return;
    }
    const insidePanel = link.closest('[data-gx-help-panel]');
    const state = insidePanel
        ? panels.get(insidePanel.dataset.gxHelpPanel)
        : link.classList.contains('help-link')
            ? [...panels.values()].find(panel => destination.origin === panel.url.origin &&
                destination.pathname.toLowerCase() === panel.url.pathname.toLowerCase())
            : null;
    if (!state || destination.origin !== state.url.origin) {
        return;
    }
    if (insidePanel) {
        const current = new URL(location.href);
        const sameHelpRequest = destination.pathname.toLowerCase() === state.url.pathname.toLowerCase() &&
            (destination.searchParams.get('version') ?? '') === (insidePanel.dataset.helpVersion ?? '') &&
            (destination.searchParams.get('culture') ?? '') === (insidePanel.dataset.helpCulture ?? '');
        const fragmentLink = link.getAttribute('href')?.startsWith('#');
        const currentDocument = destination.pathname.toLowerCase() !== state.url.pathname.toLowerCase() &&
            destination.pathname === current.pathname && destination.search === current.search;
        const localDocument = fragmentLink || sameHelpRequest || currentDocument;
        const target = localDocument ? findTarget(insidePanel, destination.hash) : null;
        if (target) {
            event.preventDefault();
            event.stopPropagation();
            target.scrollIntoView({ block: 'start' });
            if (!target.hasAttribute('tabindex')) {
                target.setAttribute('tabindex', '-1');
            }
            target.focus({ preventScroll: true });
            return;
        }
        if ((fragmentLink || currentDocument) && destination.hash) {
            event.preventDefault();
            event.stopPropagation();
            return;
        }
    }
    if (destination.pathname.toLowerCase() === state.url.pathname.toLowerCase()) {
        // Capture before Markdown fragment handling or application navigation interception.
        event.preventDefault();
        event.stopPropagation();
        state.receiver.invokeMethodAsync('OpenAsync', destination.href);
    }
}

export function initialize(receiver, url, id) {
    if (panels.has(id)) {
        throw new Error('Help panels must have unique identifiers.');
    }
    const first = panels.size === 0;
    panels.set(id, { receiver, url: new URL(url, location.href) });
    if (first) {
        document.addEventListener('click', handleClick, true);
    }
}

export function show(id, topic) {
    const state = panels.get(id);
    const root = document.getElementById(id);
    if (!state || !root) {
        return;
    }
    state.observer?.disconnect();
    state.root = root;
    state.topic = topic;
    state.observer = new MutationObserver(() => scrollToTopic(state));
    state.observer.observe(root, { childList: true, subtree: true });
    scrollToTopic(state);
}

export function close(id) {
    const state = panels.get(id);
    if (state) {
        state.observer?.disconnect();
        state.observer = null;
        state.root = null;
        state.topic = null;
    }
}

export function dispose(id) {
    close(id);
    panels.delete(id);
    if (panels.size === 0) {
        document.removeEventListener('click', handleClick, true);
    }
}
