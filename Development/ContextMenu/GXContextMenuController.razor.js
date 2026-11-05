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

const listeners = new WeakMap();

export function show(menu, trigger) {
    if (!menu.matches(':popover-open'))
    {
        menu.showPopover();
    }
    const position = () => {
        const anchor = trigger.getBoundingClientRect();
        const bounds = menu.getBoundingClientRect();
        const gap = 8;
        const maxLeft = Math.max(gap, window.innerWidth - bounds.width - gap);
        const maxTop = Math.max(gap, window.innerHeight - bounds.height - gap);
        const preferredTop = anchor.bottom + bounds.height + gap <= window.innerHeight
            ? anchor.bottom : anchor.top - bounds.height;
        menu.style.left = Math.max(gap, Math.min(anchor.right - bounds.width, maxLeft)) + 'px';
        menu.style.top = Math.max(gap, Math.min(preferredTop, maxTop)) + 'px';
    };
    position();
    if (!listeners.has(menu)) {
        listeners.set(menu, position);
        window.addEventListener('resize', position);
        window.addEventListener('scroll', position, true);
    }
}

export function hide(menu) {
    const position = listeners.get(menu);
    if (position) {
        window.removeEventListener('resize', position);
        window.removeEventListener('scroll', position, true);
        listeners.delete(menu);
    }
    if (menu.matches(':popover-open'))
    {
        menu.hidePopover();
    }
}
