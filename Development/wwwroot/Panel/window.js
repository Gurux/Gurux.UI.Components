export function resize(component) {
    window.addEventListener("resize", function () {
        setTimeout(raiseEvent, 1, component, "OnWindowResized", window.innerWidth, window.innerHeight);
    });

    function raiseEvent(comp, eventname, x, y) {
        console.log("resize");
        comp.invokeMethodAsync(eventname, x, y);
    }
}

export function top() {
    //Get location.
    var rect = document.body.getBoundingClientRect();
    return rect.top;
}
export function left(component) {
    var wnd = document.getElementById(component);
    //Get location.
    return wnd.getBoundingClientRect().left;
    /*
    var rect = document.body.getBoundingClientRect();
    return rect.left;
    */
}

export function width() {
    return window.innerWidth;
}

export function height() {
    return window.innerHeight;
}

export function initializeSplitter(panel, component) {
    const left = panel.querySelector("#leftDiv");
    const divider = panel.querySelector("#verticalDivider");
    if (!left || !divider) throw new Error("Panel splitter elements were not found.");
    let pointer = null, startX = 0, startWidth = 0, maxWidth = 0, width = 0, frame = 0;
    let previousSelection = "";
    function fitHeight() {
        if (panel.dataset.fillViewport !== "true" || !panel.getClientRects().length) return;
        const viewport = window.visualViewport;
        const bottom = viewport ? viewport.offsetTop + viewport.height : window.innerHeight;
        const height = Math.max(0, Math.floor(bottom - panel.getBoundingClientRect().top));
        const value = height + "px";
        if (panel.style.height !== value) panel.style.height = value;
    }
    const heightObserver = new ResizeObserver(fitHeight);
    for (let ancestor = panel.parentElement; ancestor; ancestor = ancestor.parentElement)
        heightObserver.observe(ancestor);
    window.addEventListener("resize", fitHeight);
    window.visualViewport?.addEventListener("resize", fitHeight);
    fitHeight();
    const right = panel.querySelector("#rightDiv");
    let minWidth = 0;
    function boxMinimum(element) {
        const style = getComputedStyle(element);
        return parseFloat(style.paddingLeft) + parseFloat(style.paddingRight) +
            parseFloat(style.borderLeftWidth) + parseFloat(style.borderRightWidth);
    }
    function updateLimits() {
        const style = getComputedStyle(panel);
        const available = Math.max(0, panel.clientWidth - parseFloat(style.paddingLeft) -
            parseFloat(style.paddingRight) - divider.getBoundingClientRect().width);
        minWidth = Math.min(available, boxMinimum(left));
        maxWidth = Math.max(minWidth, available - boxMinimum(right));
    }
    function clamp(value) { return Math.max(minWidth, Math.min(maxWidth, value)); }
    const observer = new ResizeObserver(() => {
        updateLimits();
        const current = left.getBoundingClientRect().width;
        const limited = clamp(current);
        if (Math.abs(current - limited) > 0.5) {
            width = limited;
            paint();
        }
    });
    observer.observe(panel);
    function paint() {
        frame = 0;
        left.style.flexBasis = width + "px";
    }
    function move(event) {
        if (event.pointerId !== pointer) return;
        updateLimits();
        width = clamp(startWidth + event.clientX - startX);
        if (!frame) frame = requestAnimationFrame(paint);
    }
    function finish(event) {
        if (pointer === null || (event && event.pointerId !== pointer)) return;
        if (event?.type === "pointerup") move(event);
        if (frame) { cancelAnimationFrame(frame); paint(); }
        const released = pointer;
        pointer = null;
        panel.style.userSelect = previousSelection;
        if (divider.hasPointerCapture(released)) divider.releasePointerCapture(released);
        if (event?.type === "pointerup" && component) {
            const leftSize = left.getBoundingClientRect();
            const rightSize = panel.querySelector("#rightDiv").getBoundingClientRect();
            component.invokeMethodAsync("SaveSizes", leftSize.width, leftSize.height, rightSize.width, rightSize.height)
                .catch(error => console.error("Could not save panel size.", error));
        }
    }
    function down(event) {
        if (pointer !== null || event.button !== 0) return;
        event.preventDefault();
        pointer = event.pointerId;
        startX = event.clientX;
        startWidth = width = left.getBoundingClientRect().width;
        updateLimits();
        previousSelection = panel.style.userSelect;
        panel.style.userSelect = "none";
        divider.setPointerCapture(pointer);
    }
    divider.addEventListener("pointerdown", down);
    divider.addEventListener("pointermove", move);
    divider.addEventListener("pointerup", finish);
    divider.addEventListener("pointercancel", finish);
    divider.addEventListener("lostpointercapture", finish);
    return { dispose() {
        finish();
        observer.disconnect();
        heightObserver.disconnect();
        window.removeEventListener("resize", fitHeight);
        window.visualViewport?.removeEventListener("resize", fitHeight);
        if (frame) cancelAnimationFrame(frame);
        divider.removeEventListener("pointerdown", down);
        divider.removeEventListener("pointermove", move);
        divider.removeEventListener("pointerup", finish);
        divider.removeEventListener("pointercancel", finish);
        divider.removeEventListener("lostpointercapture", finish);
    } };
}
