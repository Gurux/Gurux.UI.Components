const copyIcon = '<rect x="9" y="9" width="13" height="13" rx="2" ry="2" /><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />';
const checkIcon = '<path d="M20 6 9 17l-5-5" />';
const initialized = new WeakSet();

async function copyText(text) {
    if (navigator.clipboard) {
        await navigator.clipboard.writeText(text);
        return;
    }
    const previousFocus = document.activeElement;
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.style.cssText = 'position:fixed;left:-10000px;top:0';
    document.body.append(textarea);
    try {
        textarea.select();
        if (!document.execCommand('copy')) throw new Error('Copy failed');
    } finally {
        textarea.remove();
        previousFocus?.focus({ preventScroll: true });
    }
}

export function addCopyButton(code) {
    if (initialized.has(code)) return;
    initialized.add(code);
    const pre = code.parentElement;
    const wrapper = document.createElement('div');
    wrapper.className = 'markdown-code-block';
    pre.before(wrapper);
    wrapper.append(pre);
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'markdown-copy-button';
    wrapper.append(button);
    let timer;
    function setState(title, copied = false) {
        button.title = title;
        button.setAttribute('aria-label', title);
        button.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true" focusable="false">' +
            (copied ? checkIcon : copyIcon) + '</svg><span class="markdown-copy-tooltip">' + title + '</span>';
    }
    setState('Copy');
    button.addEventListener('click', async () => {
        clearTimeout(timer);
        button.disabled = true;
        try {
            await copyText(code.textContent);
            setState('Copied', true);
        } catch {
            setState('Copy failed');
        } finally {
            button.disabled = false;
            timer = setTimeout(() => setState('Copy'), 3000);
        }
    });
}
