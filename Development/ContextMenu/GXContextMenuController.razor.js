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
const tabs = new WeakMap();
const menuKeys = new WeakMap();
const triggers = new WeakMap();

function menuItems(menu) {
    return [...menu.querySelectorAll('[role="menuitem"]')].filter(item => !item.disabled && item.getAttribute('aria-disabled') !== 'true' && item.getClientRects().length && item.closest('[role="menu"]') === menu);
}

export function focusMenu(root, key = "Home") {
    const menu = root.matches('[role="menu"]') ? root : root.querySelector('[role="menu"]');
    if (!menu) {
        return;
    }
    const items = menuItems(menu);
    if (!items.length) {
        return;
    }
    const current = items.indexOf(document.activeElement);
    const index = key === 'End' ? items.length - 1 : key === 'Home' ? 0 : key === 'ArrowUp' ? (current - 1 + items.length) % items.length : (current + 1) % items.length;
    items[index].focus({preventScroll: true});
}

export function configureTabs(root, enabled) {
    const previous = tabs.get(root);
    if (previous) {
        root.removeEventListener('keydown',previous);
        tabs.delete(root);
    }
    if (enabled) {
        const handler = event => {
            if (!event.altKey && !event.ctrlKey && !event.metaKey && ['ArrowLeft','ArrowRight','Home','End',' '].includes(event.key) && event.target.closest('[role="tablist"]') === root) {
                event.preventDefault();
            }
        };
        root.addEventListener('keydown',handler);
        tabs.set(root,handler);
    }
}

export function configureTrigger(root, enabled) {
    const previous = triggers.get(root);
    if (previous) {
        root.removeEventListener('keydown',previous);
        triggers.delete(root);
    }
    if (enabled) {
        const handler = event => {
            if (['Enter',' ','ContextMenu'].includes(event.key) || event.key === 'F10' && event.shiftKey) {
                event.preventDefault();
            }
        };
        root.addEventListener('keydown',handler);
        triggers.set(root,handler);
    }
}

function focusBeyondMenu(trigger, menu, event) {
    const items = [...document.querySelectorAll('button,[href],input,select,textarea,[tabindex]')].filter(item => !item.disabled && item.tabIndex >= 0 && item.getClientRects().length && !menu.contains(item));
    const index = items.indexOf(trigger);
    const next = items[index + (event.shiftKey ? -1 : 1)];
    if (next) {
        event.preventDefault();
        next.focus();
    } else {
        trigger.focus();
    }
}

export function configureMenu(root, enabled) {
    const previous = menuKeys.get(root);
    if (previous) {
        root.removeEventListener('keydown',previous);
        menuKeys.delete(root);
    }
    if (enabled) {
        const handler = event => {
            if (event.key === 'Tab') {
                const host = root.closest('.gx-menu-control:not(.gx-menu-submenu)') ?? root;
                const popup = host.querySelector('[role="menu"]');
                const trigger = host.querySelector('.gx-menu-toggle');
                if (popup && trigger) {
                    focusBeyondMenu(trigger,popup,event);
                    host.querySelector('.gx-menu-dismiss')?.click();
                    event.stopPropagation();
                }
            }
            if (['ArrowUp','ArrowDown','ArrowLeft','ArrowRight','Home','End'].includes(event.key)) {
                event.preventDefault();
            }
        };
        root.addEventListener('keydown',handler);
        menuKeys.set(root,handler);
    }
}

export function show(menu, trigger, options = {}, reference = null) {
    let state = listeners.get(menu);
    const opening = !state;
    if (!state) {
        state = { options, trigger };
        state.position = () => {
            const gap = 8;
            if (state.options.sheet) {
                menu.style.left = gap + 'px';
                menu.style.right = gap + 'px';
                menu.style.top = 'auto';
                menu.style.bottom = 'max(8px, env(safe-area-inset-bottom))';
                menu.style.width = 'calc(100vw - 16px)';
                return;
            }
            menu.style.right = 'auto';
            menu.style.bottom = 'auto';
            menu.style.width = '';
            const anchor = state.trigger.getBoundingClientRect();
            const bounds = menu.getBoundingClientRect();
            const maxLeft = Math.max(gap, window.innerWidth - bounds.width - gap);
            const maxTop = Math.max(gap, window.innerHeight - bounds.height - gap);
            const preferredTop = anchor.bottom + bounds.height + gap <= window.innerHeight ? anchor.bottom : anchor.top - bounds.height;
            menu.style.left = Math.max(gap, Math.min(anchor.right - bounds.width, maxLeft)) + 'px';
            menu.style.top = Math.max(gap, Math.min(preferredTop, maxTop)) + 'px';
        };
        state.key = event => {
            if (state.options.adaptive && event.key === 'Tab') {
                focusBeyondMenu(state.trigger,menu,event);
                Promise.resolve(state.reference?.invokeMethodAsync("DismissMenu")).catch(() => {});
                return;
            }
            if (state.options.adaptive && ['ArrowUp','ArrowDown','Home','End',' '].includes(event.key)) {
                event.preventDefault();
                if (event.key !== ' ') {
                    focusMenu(menu,event.key);
                    event.stopPropagation();
                }
                if (event.key === ' ' && event.target.matches('[role="menuitem"]')) {
                    event.target.click();
                }
            }
            if (state.options.adaptive && event.key === 'Enter' && event.target.matches('[role="menuitem"]')) {
                event.preventDefault();
                event.target.click();
            }
        };
        state.outside = event => {
            if (state.options.adaptive && !menu.contains(event.target) && !state.trigger.contains(event.target)) {
                Promise.resolve(state.reference?.invokeMethodAsync("DismissMenu")).catch(() => {});
            }
        };
        document.addEventListener('pointerdown',state.outside);
        listeners.set(menu,state);
        window.addEventListener('resize',state.position);
        window.addEventListener('scroll',state.position,true);
        menu.addEventListener('keydown',state.key);
    }
    state.options = options;
    state.reference = reference;
    state.trigger = trigger;
    if (typeof menu.showPopover === 'function' && !menu.matches(':popover-open')) {
        menu.showPopover();
    }
    state.position();
    if (opening && options.adaptive) {
        focusMenu(menu);
    }
}

export function hide(menu) {
    const state = listeners.get(menu);
    if (state) {
        window.removeEventListener('resize',state.position);
        window.removeEventListener('scroll',state.position,true);
        menu.removeEventListener('keydown',state.key);
        document.removeEventListener('pointerdown',state.outside);
        listeners.delete(menu);
    }
    if (typeof menu.hidePopover === 'function' && menu.matches(':popover-open')) {
        menu.hidePopover();
    }
}
