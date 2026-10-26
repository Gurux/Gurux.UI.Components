const maps = new WeakMap();
const disposed = new WeakSet();
let leaflet;

function loadLeaflet() {
    return leaflet ??= (async () => {
        // Static web assets expose wwwroot contents directly under the package path.
        const assets = new URL('./leaflet/', import.meta.url);
        await new Promise((resolve, reject) => {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = new URL('leaflet.css', assets).href;
            link.onload = resolve;
            link.onerror = () => { link.remove(); reject(new Error('Unable to load Leaflet CSS.')); };
            document.head.append(link);
        });
        if (!globalThis.L) {
            await new Promise((resolve, reject) => {
                const script = document.createElement('script');
                script.src = new URL('leaflet.js', assets).href;
                script.onload = resolve;
                script.onerror = () => { script.remove(); reject(new Error('Unable to load Leaflet.')); };
                document.head.append(script);
            });
        }
        return globalThis.L;
    })().catch(error => { leaflet = undefined; throw error; });
}

function externalUrl(value) {
    try {
        const url = new URL(value);
        return url.protocol === 'https:' || url.protocol === 'http:' ? url.href : null;
    } catch { return null; }
}

function tileUrl(proxyUrl) {
    const source = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
    if (!proxyUrl?.trim()) return source;
    const proxy = new URL(proxyUrl, document.baseURI);
    if (!['https:', 'http:'].includes(proxy.protocol)) throw new Error('ProxyUrl must use HTTP or HTTPS.');
    proxy.searchParams.set('url', source);
    // Leaflet must substitute coordinates after encoding the target URL in the proxy query.
    return proxy.href.replaceAll('%7B', '{').replaceAll('%7D', '}');
}

export async function update(element, latitude, longitude, zoom, markers, proxyUrl) {
    const L = await loadLeaflet();
    if (!element?.isConnected || disposed.has(element)) return;
    const tilesUrl = tileUrl(proxyUrl);
    let state = maps.get(element);
    if (!state) {
        const map = L.map(element).setView([latitude, longitude], zoom);
        const tiles = L.tileLayer(tilesUrl, {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);
        state = { map, tiles, tilesUrl, layer: L.layerGroup().addTo(map), latitude, longitude, zoom };
        state.resize = new ResizeObserver(() => map.invalidateSize());
        state.resize.observe(element);
        state.removed = new MutationObserver(() => {
            if (!element.isConnected) dispose(element);
        });
        state.removed.observe(document.documentElement, { childList: true, subtree: true });
        maps.set(element, state);
    }
    if (state.tilesUrl !== tilesUrl) {
        state.tiles.setUrl(tilesUrl);
        state.tilesUrl = tilesUrl;
    }
    if (state.latitude !== latitude || state.longitude !== longitude || state.zoom !== zoom) {
        state.map.setView([latitude, longitude], zoom);
        Object.assign(state, { latitude, longitude, zoom });
    }
    state.layer.clearLayers();
    for (const item of markers ?? []) {
        const marker = L.marker([item.latitude, item.longitude], {
            title: item.title ?? '', alt: item.title || 'Map marker', keyboard: true
        }).addTo(state.layer);
        const url = externalUrl(item.url);
        if (url) marker.on('click', () => window.open(url, '_blank', 'noopener,noreferrer'));
    }
}

export function dispose(element) {
    if (!element) return;
    disposed.add(element);
    const state = maps.get(element);
    if (!state) return;
    state.resize.disconnect();
    state.removed.disconnect();
    state.map.remove();
    maps.delete(element);
}
