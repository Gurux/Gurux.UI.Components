// Browser-only appearance state. One shared stylesheet owner per document.
let shared;

export function detectPlatform(client = globalThis.navigator ?? {}) {
    const hint = client.userAgentData?.platform ?? "";
    const agent = client.userAgent ?? "";
    const platform = client.platform ?? "";
    if (/iOS/i.test(hint) || /iPhone|iPad|iPod/i.test(agent) || (/Mac/i.test(platform + hint) && client.maxTouchPoints > 1)) {
        return "iOS";
    }
    if (/Android/i.test(hint + agent)) {
        return "Android";
    }
    if (/Windows|Win32|Win64/i.test(hint + platform + agent)) {
        return "Windows";
    }
    if (/macOS|Macintosh|MacIntel/i.test(hint + platform + agent)) {
        return "macOS";
    }
    if (/Linux/i.test(hint + platform + agent)) {
        return "Linux";
    }
    return "Unknown";
}

function notifyViewport(state) {
    const width = Math.round(window.innerWidth || 0);
    const coarse = !!window.matchMedia?.("(pointer: coarse)")?.matches;
    for (const owner of state.owners) {
        Promise.resolve(owner.dotnet.invokeMethodAsync("ReceiveViewport", width, coarse)).catch(() => {});
    }
}

function notify(state) {
    notifyViewport(state);
    const platform = detectPlatform();
    for (const owner of state.owners) {
        // A disconnected Server circuit must not create unhandled rejections.
        Promise.resolve(owner.dotnet.invokeMethodAsync("ReceiveEnvironment", platform, !!state.media?.matches)).catch(() => {});
    }
}

function applyColor(state) {
    const mode = state.settings?.colorMode;
    const resolved = mode === "Dark" || (mode === "Auto" && state.media?.matches) ? "dark" : "light";
    document.documentElement.setAttribute("data-bs-theme", resolved);
}

function localUrl(path, allowExternal) {
    const base = new URL(document.baseURI);
    const url = new URL(path, base);
    if ((!allowExternal && (url.origin !== base.origin || url.search || url.hash || !url.pathname.toLowerCase().endsWith(".css"))) || !["https:", "http:"].includes(url.protocol) || url.username || url.password) {
        throw new Error("Theme stylesheet must be a local CSS asset.");
    }
    return url.href;
}

function restoreVariables(state) {
    const style = document.documentElement.style;
    for (const [name, previous] of state.variables) {
        if (previous.value) {
            style.setProperty(name, previous.value, previous.priority);
        } else {
            style.removeProperty(name);
        }
    }
    state.variables.clear();
}

function applyVariables(state, variables = {}) {
    restoreVariables(state);
    const style = document.documentElement.style;
    for (const [name, value] of Object.entries(variables)) {
        if (!/^--[a-zA-Z0-9_-]+$/.test(name) || typeof value !== "string") {
            continue;
        }
        state.variables.set(name, { value: style.getPropertyValue(name), priority: style.getPropertyPriority(name) });
        style.setProperty(name, value);
    }
}

function load(state, href, replaceBootstrap = true) {
    const cacheKey = `${replaceBootstrap ? "base" : "overlay"}:${href}`;
    if (state.cache.has(cacheKey)) {
        return state.cache.get(cacheKey);
    }
    const link = document.createElement("link");
    link.rel = "stylesheet";
    link.media = "not all";
    link.dataset.gxAppearance = "theme";
    state.sheets.add(link);
    const promise = new Promise((resolve, reject) => {
        let timer;
        const finish = (success) => {
            clearTimeout(timer);
            link.removeEventListener("load", loaded);
            link.removeEventListener("error", failed);
            state.pending.delete(link);
            if (success && !state.disposed) {
                resolve(link);
            } else {
                state.cache.delete(cacheKey);
                link.remove();
                state.sheets.delete(link);
                reject(new Error("Unable to load theme stylesheet."));
            }
        };
        const loaded = () => finish(true);
        const failed = () => finish(false);
        state.pending.set(link, failed);
        link.addEventListener("load", loaded);
        link.addEventListener("error", failed);
        timer = setTimeout(failed, 15000);
        link.href = href;
        // Keep host component and application overrides after the Bootstrap stylesheet.
        if (!replaceBootstrap) {
            // Preserve additive user CSS precedence over application styles.
            document.head.appendChild(link);
        } else if (state.host?.parentNode) {
            state.host.parentNode.insertBefore(link, state.host);
        } else {
            document.head.insertBefore(link, state.layout);
        }
    });
    state.cache.set(cacheKey, promise);
    return promise;
}

function makeShared(options) {
    const selector = options.bootstrapStylesheetSelector || "link[data-gx-bootstrap]";
    let hosts = [...document.querySelectorAll(selector)];
    if (hosts.length === 0) {
        hosts = [...document.querySelectorAll('link[rel="stylesheet"]')].filter(link => /(?:^|\/)bootstrap(?:\.min)?\.css(?:[?#]|$)|bootswatch/i.test(link.href));
    }
    if (hosts.length > 1) {
        throw new Error("Multiple Bootstrap stylesheets found. Mark exactly one with data-gx-bootstrap and remove duplicate Bootstrap imports.");
    }
    const media = window.matchMedia?.("(prefers-color-scheme: dark)");
    const layout = document.createElement("link");
    layout.rel = "stylesheet";
    layout.href = new URL("_content/Gurux.UI.Components/Appearance/layout.css", document.baseURI).href;
    layout.dataset.gxAppearance = "layout";
    document.head.appendChild(layout);
    const host = hosts[0];
    const state = { owners: new Set(), variables: new Map(), cache: new Map(), sheets: new Set(), pending: new Map(), media, layout, host, hostDisabled: host?.disabled, previousColor: document.documentElement.getAttribute("data-bs-theme"), version: 0, disposed: false, selector };
    state.resized = () => {
        clearTimeout(state.resizeTimer);
        state.resizeTimer = setTimeout(() => { if (!state.disposed) { notifyViewport(state); } }, 50);
    };
    window.addEventListener?.("resize", state.resized);
    state.changed = () => {
        if (state.settings) {
            applyColor(state);
        }
        notify(state);
    };
    if (media?.addEventListener) {
        media.addEventListener("change", state.changed);
    } else {
        media?.addListener?.(state.changed);
    }
    return state;
}

export function create(dotnet, options = {}) {
    if (!shared) {
        shared = makeShared(options);
    }
    const state = shared;
    const selector = options.bootstrapStylesheetSelector || "link[data-gx-bootstrap]";
    if (state.selector !== selector) {
        throw new Error("Appearance providers must share the same Bootstrap stylesheet selector.");
    }
    const owner = { dotnet, disposed: false };
    state.owners.add(owner);
    notify(state);
    return {
        async apply(settings) {
            if (owner.disposed) {
                return;
            }
            const defaultHref = localUrl("_content/Gurux.UI.Components/Appearance/themes/default.min.css", false);
            const href = settings.cssUrl ? localUrl(settings.cssUrl, settings.allowExternalStylesheet === true) : state.host ? null : defaultHref;
            const replaceBootstrap = settings.replaceBootstrap !== false;
            const version = ++state.version;
            const restoreHost = !href || (settings.themeId === "default" && state.host);
            const next = restoreHost || (!settings.cssUrl && !replaceBootstrap) ? null : await load(state, href, replaceBootstrap);
            const base = !replaceBootstrap && !state.host ? await load(state, defaultHref) : null;
            if (state.disposed || version !== state.version) {
                return;
            }
            for (const promise of state.cache.values()) {
                // Every loaded theme remains cached but only one stylesheet applies.
                promise.then(link => { link.media = link === next || link === base ? "all" : "not all"; }).catch(() => {});
            }
            if (next) {
                next.media = "all";
            }
            if (state.host) {
                state.host.disabled = restoreHost || !replaceBootstrap ? state.hostDisabled : true;
            }
            applyVariables(state, settings.cssVariables);
            state.settings = { ...settings };
            applyColor(state);
        },
        dispose() {
            if (owner.disposed) {
                return;
            }
            owner.disposed = true;
            state.owners.delete(owner);
            if (state.owners.size !== 0) {
                return;
            }
            state.disposed = true;
            clearTimeout(state.resizeTimer);
            window.removeEventListener?.("resize", state.resized);
            ++state.version;
            if (state.media?.removeEventListener) {
                state.media.removeEventListener("change", state.changed);
            } else {
                state.media?.removeListener?.(state.changed);
            }
            for (const cancel of [...state.pending.values()]) {
                cancel();
            }
            for (const link of state.sheets) {
                link.remove();
            }
            state.sheets.clear();
            state.layout.remove();
            restoreVariables(state);
            if (state.host) {
                state.host.disabled = state.hostDisabled;
            }
            if (state.previousColor === null) {
                document.documentElement.removeAttribute("data-bs-theme");
            } else {
                document.documentElement.setAttribute("data-bs-theme", state.previousColor);
            }
            if (shared === state) {
                shared = undefined;
            }
        }
    };
}
