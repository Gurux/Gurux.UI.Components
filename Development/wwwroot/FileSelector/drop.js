export function initializeFileDropZone(component, inputFile) {
    function onDragHover(e) {
        e.preventDefault();
        component.classList.add("hover");
    }

    function onDragLeave(e) {
        e.preventDefault();
        component.classList.remove("hover");
    }

    function onDrop(e) {
        e.preventDefault();
        component.classList.remove("hover");
        inputFile.files = e.dataTransfer.files;
        inputFile.dispatchEvent(new Event("change", { bubbles: true }));
    }

    function onPaste(e) {
        inputFile.files = e.clipboardData.files;
        inputFile.dispatchEvent(new Event("change", { bubbles: true }));
    }

    component.addEventListener("dragenter", onDragHover);
    component.addEventListener("dragover", onDragHover);
    component.addEventListener("dragleave", onDragLeave);
    component.addEventListener("drop", onDrop);
    component.addEventListener("paste", onPaste);

    return {
        dispose: () => {
            component.removeEventListener("dragenter", onDragHover);
            component.removeEventListener("dragover", onDragHover);
            component.removeEventListener("dragleave", onDragLeave);
            component.removeEventListener("drop", onDrop);
            component.removeEventListener("paste", onPaste);
        }
    };
}
