import hljs from './highlight/core.min.js';
import { addCopyButton } from './copy.js';
import csharp from './highlight/languages/csharp.min.js';
import json from './highlight/languages/json.min.js';
import sql from './highlight/languages/sql.min.js';
import bash from './highlight/languages/bash.min.js';
import powershell from './highlight/languages/powershell.min.js';
import xml from './highlight/languages/xml.min.js';
import java from './highlight/languages/java.min.js';
import pascal from './highlight/languages/delphi.min.js';
import go from './highlight/languages/go.min.js';
import python from './highlight/languages/python.min.js';
import c from './highlight/languages/c.min.js';
import cpp from './highlight/languages/cpp.min.js';
import javascript from './highlight/languages/javascript.min.js';

for (const [name, grammar] of Object.entries({ csharp, json, sql, bash, powershell,
    xml, java, pascal, go, python, c, cpp, javascript })) {
    hljs.registerLanguage(name, grammar);
}
hljs.registerAliases('html', { languageName: 'xml' });

// Track actual nodes: Blazor can replace the markup when Content or Source changes.
const highlighted = new WeakMap();
export function highlight(root) {
    for (const code of root.querySelectorAll('pre > code')) {
        addCopyButton(code);
        const languageClass = [...code.classList].find(value => value.startsWith('language-'));
        const language = languageClass?.slice('language-'.length);
        if (!language || !hljs.getLanguage(language)) continue;
        const text = code.textContent;
        const previous = highlighted.get(code);
        if (previous?.text === text && previous.language === language) continue;
        // highlight() escapes source text; code is never interpreted as executable HTML.
        code.innerHTML = hljs.highlight(text, { language, ignoreIllegals: true }).value;
        code.classList.add('hljs');
        highlighted.set(code, { text, language });
    }
}
