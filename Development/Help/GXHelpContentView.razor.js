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

// HTML is parsed in an inert document and copied through an explicit allowlist.
const tags = new Set("a abbr b blockquote br caption code col colgroup dd del details div dl dt em figcaption figure h1 h2 h3 h4 h5 h6 hr i img kbd li main mark ol p pre s samp section small span strong sub summary sup table tbody td th thead tfoot tr u ul var article".split(" "));
const discard = new Set("script style form iframe frame frameset embed object svg math template noscript head meta link base title input button textarea select option audio video source track canvas".split(" "));

function safeUrl(value, base, fragment) {
    const trimmed = value.trim();
    if (fragment && trimmed.startsWith("#")) {
        return trimmed;
    }
    try {
        const url = base ? new URL(trimmed, base) : new URL(trimmed);
        if ((url.protocol === "http:" || url.protocol === "https:") && !url.username && !url.password) {
            return url.href;
        }
    } catch {
    }
    return null;
}

export function sanitize(html, baseAddress, articleSelector) {
    const parsed = new DOMParser().parseFromString(html || "", "text/html");
    let root = parsed.body;
    if (articleSelector) {
        try {
            root = parsed.querySelector(articleSelector);
        } catch {
            root = null;
        }
    }
    const output = document.implementation.createHTMLDocument("");
    const base = baseAddress ? safeUrl(baseAddress, null, false) : null;
    function copy(node, parent, depth = 0) {
        if (depth > 128) {
            return;
        }
        if (node.nodeType === Node.TEXT_NODE) {
            parent.appendChild(output.createTextNode(node.textContent));
            return;
        }
        if (node.nodeType !== Node.ELEMENT_NODE) {
            return;
        }
        const name = node.localName.toLowerCase();
        if (discard.has(name) || node.namespaceURI !== "http://www.w3.org/1999/xhtml") {
            return;
        }
        let target = parent;
        if (tags.has(name)) {
            target = output.createElement(name);
            parent.appendChild(target);
            for (const attr of ["id", "title", "lang", "dir", "alt", "colspan", "rowspan", "scope"]) {
                if (node.hasAttribute(attr)) {
                    target.setAttribute(attr, node.getAttribute(attr));
                }
            }
            const attribute = name === "a" ? "href" : name === "img" ? "src" : null;
            if (attribute && node.hasAttribute(attribute)) {
                const url = safeUrl(node.getAttribute(attribute), base, name === "a");
                if (url) {
                    target.setAttribute(attribute, url);
                    if (name === "a" && !url.startsWith("#")) {
                        target.setAttribute("target", "_blank");
                        target.setAttribute("rel", "noopener noreferrer");
                    }
                }
            }
            if (name === "img") {
                target.setAttribute("referrerpolicy", "no-referrer");
            }
        }
        for (const child of node.childNodes) {
            copy(child, target, depth + 1);
        }
    }
    if (root) {
        copy(root, output.body);
    }
    const csp = "default-src 'none'; script-src 'none'; connect-src 'none'; frame-src 'none'; object-src 'none'; form-action 'none'; base-uri 'none'; img-src http: https:; style-src 'unsafe-inline'";
    return '<!doctype html><html><head><meta charset="utf-8"><meta http-equiv="Content-Security-Policy" content="' + csp + '"><meta name="referrer" content="no-referrer"><style>body{font:16px/1.6 system-ui,sans-serif;margin:1rem;color:#222;overflow-wrap:anywhere}img{max-width:100%;height:auto}table{border-collapse:collapse;max-width:100%}td,th{border:1px solid #ccc;padding:.4rem}pre{overflow:auto;background:#f5f5f5;padding:.75rem}a{color:#075db0}</style></head><body>' + output.body.innerHTML + '</body></html>';
}
