// SPDX-License-Identifier: GPL-2.0-only OR LicenseRef-Gurux-Commercial

function cameraError(error) {
    const messages = {
        NotAllowedError: "Camera permission was denied. Allow camera access in your browser settings.",
        SecurityError: "Camera access is blocked by browser security or Permissions-Policy.",
        NotFoundError: "No camera is available.",
        NotReadableError: "The camera is unavailable or already in use.",
        OverconstrainedError: "The requested camera is unavailable."
    };
    return new Error(messages[error?.name] ?? error?.message ?? "Unable to access the camera.");
}

/** Creates one independent controller. No streams are shared between instances. */
export function create(video, image) {
    let stream = null;
    let revision = 0;
    let disposed = false;
    let facing = "environment";
    let photoUrl = null;
    const waits = new Set();

    function releaseTracks(value) {
        if (value) {
            for (const track of value.getTracks()) {
                track.stop();
            }
        }
    }

    function retake() {
        if (photoUrl !== null) {
            URL.revokeObjectURL(photoUrl);
            photoUrl = null;
        }
        image.removeAttribute("src");
        image.hidden = true;
        video.hidden = false;
    }

    function stop() {
        ++revision;
        for (const cancel of waits) {
            cancel();
        }
        waits.clear();
        releaseTracks(stream);
        stream = null;
        video.pause();
        video.srcObject = null;
    }

    function ready(token) {
        if (video.readyState >= 2 && video.videoWidth > 0 && video.videoHeight > 0) {
            return Promise.resolve(true);
        }
        return new Promise((resolve, reject) => {
            const finish = (error) => {
                video.removeEventListener("loadeddata", loaded);
                video.removeEventListener("error", failed);
                waits.delete(cancel);
                if (error) {
                    reject(error);
                } else {
                    resolve(!disposed && token === revision);
                }
            };
            const loaded = () => finish();
            const failed = () => finish(new Error("The camera preview could not be loaded."));
            const cancel = () => finish();
            waits.add(cancel);
            video.addEventListener("loadeddata", loaded, { once: true });
            video.addEventListener("error", failed, { once: true });
        });
    }

    async function start(selectedFacing, deviceId) {
        if (disposed) {
            throw new Error("The camera component is disposed.");
        }
        if (!["environment", "user"].includes(selectedFacing)) {
            throw new Error("Invalid camera facing mode.");
        }
        stop();
        retake();
        const token = revision;
        let acquired = null;
        try {
            if (!navigator.mediaDevices?.getUserMedia) {
                throw new Error("Camera access is unsupported. Use HTTPS or localhost and a supported browser.");
            }
            acquired = await navigator.mediaDevices.getUserMedia({ audio: false,
                video: deviceId ? { deviceId: { exact: deviceId } } : { facingMode: { ideal: selectedFacing } } });
            if (disposed || token !== revision) {
                releaseTracks(acquired);
                return false;
            }
            stream = acquired;
            facing = selectedFacing;
            video.srcObject = acquired;
            video.muted = true;
            video.playsInline = true;
            await video.play();
            if (disposed || token !== revision) {
                return false;
            }
            return await ready(token);
        } catch (error) {
            releaseTracks(acquired);
            if (disposed || token !== revision) {
                return false;
            }
            stop();
            throw cameraError(error);
        }
    }

    async function switchCamera(initialFacing) {
        if (disposed) {
            throw new Error("The camera component is disposed.");
        }
        const token = revision;
        const current = stream?.getVideoTracks()[0]?.getSettings();
        let devices;
        try {
            devices = navigator.mediaDevices?.enumerateDevices
                ? (await navigator.mediaDevices.enumerateDevices()).filter(device => device.kind === "videoinput" && device.deviceId)
                : [];
        } catch (error) {
            if (!disposed && token === revision) {
                stop();
            }
            throw cameraError(error);
        }
        if (disposed || token !== revision) {
            return false;
        }
        if (devices.length > 1 && current?.deviceId) {
            const index = devices.findIndex(device => device.deviceId === current.deviceId);
            return await start(facing, devices[(index + 1) % devices.length].deviceId);
        }
        const previous = current?.facingMode ?? (stream ? facing : initialFacing);
        return await start(previous === "environment" ? "user" : "environment");
    }

    async function capture(options) {
        if (disposed || !stream || video.videoWidth <= 0 || video.videoHeight <= 0) {
            throw new Error("Start the camera and wait for the preview before taking a photo.");
        }
        if (!["image/png", "image/jpeg", "image/webp"].includes(options.contentType)) {
            throw new Error("Unsupported image format. Choose PNG, JPEG or WebP.");
        }
        if (!Number.isFinite(options.quality) || options.quality < 0 || options.quality > 1) {
            throw new Error("Image quality must be between 0 and 1.");
        }
        if (!Number.isInteger(options.maxWidth) || options.maxWidth <= 0 ||
            !Number.isInteger(options.maxHeight) || options.maxHeight <= 0) {
            throw new Error("Maximum image dimensions must be positive integers.");
        }
        const token = revision;
        const scale = Math.min(1, options.maxWidth / video.videoWidth, options.maxHeight / video.videoHeight);
        const canvas = document.createElement("canvas");
        canvas.width = Math.max(1, Math.floor(video.videoWidth * scale));
        canvas.height = Math.max(1, Math.floor(video.videoHeight * scale));
        const context = canvas.getContext("2d");
        if (!context) {
            throw new Error("Canvas image capture is unavailable.");
        }
        context.drawImage(video, 0, 0, canvas.width, canvas.height);
        const timestamp = new Date().toISOString();
        const blob = await new Promise(resolve => canvas.toBlob(resolve, options.contentType, options.quality));
        if (disposed || token !== revision) {
            throw new Error("The camera capture was canceled.");
        }
        if (!blob || !["image/png", "image/jpeg", "image/webp"].includes(blob.type)) {
            throw new Error("The browser could not encode the image in a supported format.");
        }
        retake();
        if (options.showPreview) {
            photoUrl = URL.createObjectURL(blob);
            image.src = photoUrl;
            image.hidden = false;
            video.hidden = true;
        }
        return { stream: DotNet.createJSStreamReference(blob), contentType: blob.type,
            width: canvas.width, height: canvas.height, timestamp };
    }

    // DOM cleanup also works when a Server circuit can no longer call JavaScript.
    const observer = typeof MutationObserver === "function" ? new MutationObserver(() => {
        if (!video.isConnected) {
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
        stop();
        retake();
        observer?.disconnect();
        globalThis.removeEventListener?.("pagehide", dispose);
    }

    return { start, stop, switchCamera, capture, retake, dispose };
}
