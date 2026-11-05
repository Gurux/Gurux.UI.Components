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
