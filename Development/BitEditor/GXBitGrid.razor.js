// --------------------------------------------------------------------------
// Gurux Ltd
//
// Copyright (c) Gurux Ltd.
//
// This file is part of Gurux Device Framework.
//
// Commit: $Format:%H$
// Date:   $Format:%cI$
//
// This software is available under either of the following licenses:
//
// 1. GNU General Public License, version 2 only (GPL-2.0-only).
//    See the LICENSE file included with this distribution.
//
// 2. A commercial license granted by Gurux Ltd under a separate written
//    agreement. Use under this option requires a valid commercial license
//    and is governed by the terms of that agreement.
//
// If you have not obtained a commercial license from Gurux Ltd,
// use of this software is governed by GPL-2.0-only.
//
// Commercial licensing: https://www.gurux.fi/
//
// SPDX-License-Identifier: GPL-2.0-only OR LicenseRef-Gurux-Commercial
// --------------------------------------------------------------------------

const observers = new WeakMap();
export function observe(element, receiver) {
    disconnect(element);
    let lastWidth = -1;
    let pendingFocus;
    let focusTimer;
    const stopFocus = () => { pendingFocus?.disconnect(); pendingFocus = undefined; clearTimeout(focusTimer); };
    const observer = new ResizeObserver(entries => {
        const width = entries[0].contentRect.width;
        if (Math.abs(width - lastWidth) >= 1) {
            lastWidth = width;
            receiver.invokeMethodAsync('Resize', width).catch(() => {});
        }
    });
    const keydown = event => {
        const index = Number(event.target.dataset.displayIndex);
        if (!Number.isInteger(index)) {
            return;
        }
        const style = getComputedStyle(element);
        const columns = parseInt(style.getPropertyValue('--bits-per-row'), 10);
        const count = Number(element.dataset.bitCount);
        const offsets = { ArrowLeft: -1, ArrowRight: 1, ArrowUp: -columns, ArrowDown: columns, Home: -index, End: count - index - 1 };
        if (!(event.key in offsets)) {
            return;
        }
        event.preventDefault();
        stopFocus();
        const offset = offsets[event.key];
        let target = Math.max(0, Math.min(count - 1, index + offset));
        const direction = offset < 0 ? -1 : 1;
        const tryFocus = () => {
            while (target >= 0 && target < count) {
                const button = element.querySelector(`[data-display-index="${target}"]`);
                if (!button) {
                    return false;
                }
                if (!button.disabled) { button.focus(); stopFocus(); return true; }
                target += direction;
            }
            stopFocus();
            return true;
        };
        if (!tryFocus()) {
            // Bring a virtualized destination row into view before focusing its bit.
            pendingFocus = new MutationObserver(tryFocus);
            pendingFocus.observe(element, { childList: true, subtree: true });
            element.scrollTop = Math.floor(target / columns) * parseFloat(style.getPropertyValue('--bit-row-height'));
            focusTimer = setTimeout(stopFocus, 3000);
        }
    };
    observer.observe(element);
    element.addEventListener('keydown', keydown);
    observers.set(element, { observer, keydown, stopFocus });
}
export function disconnect(element) {
    const state = observers.get(element);
    if (state) {
        state.observer.disconnect();
        state.stopFocus();
        element.removeEventListener('keydown', state.keydown);
        observers.delete(element);
    }
}
