import { openDialog, closeDialog } from '../GXDialog.razor.js';

const panels = new Map();

export function open(root, key) {
    if (!root?.isConnected || panels.has(key)) {
        return;
    }
    const previous = document.activeElement;
    const modal = !!document.querySelector('.modal.show[role="dialog"], .gx-side-panel[aria-modal="true"]');
    panels.set(key, { previous, modal });
    if (modal) {
        root.setAttribute('aria-modal', 'true');
        openDialog(root, key);
    }
    root.focus({ preventScroll: true });
}

export function close(key) {
    const state = panels.get(key);
    if (!state) {
        return;
    }
    panels.delete(key);
    if (state.modal) {
        closeDialog(key);
    }
    if (state.previous?.isConnected) {
        state.previous.focus({ preventScroll: true });
    }
}
