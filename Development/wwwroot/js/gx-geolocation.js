// SPDX-License-Identifier: GPL-2.0-only OR LicenseRef-Gurux-Commercial

function positionResult(position) {
    const coordinates = position.coords;
    const optional = value => Number.isFinite(value) ? value : null;
    return {
        latitude: coordinates.latitude, longitude: coordinates.longitude, accuracy: coordinates.accuracy,
        altitude: optional(coordinates.altitude), altitudeAccuracy: optional(coordinates.altitudeAccuracy),
        heading: optional(coordinates.heading), speed: optional(coordinates.speed),
        timestamp: new Date(position.timestamp).toISOString()
    };
}

function locationError(error) {
    const messages = {
        1: "Location permission was denied. Allow location access in your browser settings.",
        2: "The device location is unavailable.",
        3: "The location request timed out."
    };
    return messages[error?.code] ?? error?.message ?? "Unable to obtain the device location.";
}

/** Creates a controller with its own watcher and component callback reference. */
export function create(element, callback) {
    let disposed = false;
    let watchId = null;
    let revision = 0;
    const pending = new Set();

    function api(options) {
        if (disposed) {
            throw new Error("The geolocation component is disposed.");
        }
        if (!navigator.geolocation) {
            throw new Error("Geolocation is unsupported. Use HTTPS or localhost and a supported browser.");
        }
        if (!Number.isInteger(options.timeout) || options.timeout < 0 ||
            !Number.isInteger(options.maximumAge) || options.maximumAge < 0) {
            throw new Error("Location timeout and maximum age must be nonnegative milliseconds.");
        }
        return navigator.geolocation;
    }

    async function getLocation(options) {
        const geolocation = api(options);
        return await new Promise((resolve, reject) => {
            const cancel = () => reject(new Error("The geolocation component is disposed."));
            pending.add(cancel);
            const finish = action => {
                if (pending.delete(cancel) && !disposed) {
                    action();
                }
            };
            try {
                geolocation.getCurrentPosition(
                    position => finish(() => resolve(positionResult(position))),
                    error => finish(() => reject(new Error(locationError(error)))), options);
            } catch (error) {
                finish(() => reject(error));
            }
        });
    }

    function stopWatching() {
        ++revision;
        if (watchId !== null) {
            navigator.geolocation?.clearWatch(watchId);
            watchId = null;
        }
    }

    function startWatching(options, generation) {
        const geolocation = api(options);
        stopWatching();
        const token = revision;
        async function notify(method, value) {
            if (disposed || token !== revision) {
                return;
            }
            try {
                await callback.invokeMethodAsync(method, value, generation);
            } catch {
                // A removed/disconnected .NET component must not retain a watcher.
                dispose();
            }
        }
        watchId = geolocation.watchPosition(
            position => { void notify("ReceiveLocation", positionResult(position)); },
            error => { void notify("ReceiveError", locationError(error)); }, options);
    }

    const observer = element && typeof MutationObserver === "function" ? new MutationObserver(() => {
        if (!element.isConnected) {
            dispose();
        }
    }) : null;
    observer?.observe(document.documentElement, { childList: true, subtree: true });
    globalThis.addEventListener?.("pagehide", dispose);

    function dispose() {
        if (disposed) {
            return;
        }
        disposed = true;
        stopWatching();
        for (const cancel of pending) {
            cancel();
        }
        pending.clear();
        observer?.disconnect();
        globalThis.removeEventListener?.("pagehide", dispose);
    }

    return { getLocation, startWatching, stopWatching, dispose };
}
