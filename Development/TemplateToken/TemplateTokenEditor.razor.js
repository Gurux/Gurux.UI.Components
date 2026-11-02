export function insertToken(textArea, token) {
    const start = textArea.selectionStart;
    const end = textArea.selectionEnd;
    textArea.setRangeText(token, start, end, "end");
    textArea.focus();
    return textArea.value;
}
