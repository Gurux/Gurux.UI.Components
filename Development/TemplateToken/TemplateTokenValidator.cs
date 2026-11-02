using System.Text.RegularExpressions;

namespace Gurux.UI.Components.TemplateToken;

/// <summary>Checks token syntax and names without expanding template text.</summary>
public static partial class TemplateTokenValidator
{
    /// <summary>Returns distinct errors for unknown or malformed token references; ordinary bracketed text is allowed.</summary>
    public static IReadOnlyList<string> Validate(string text, IEnumerable<TemplateTokenDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(definitions);
        var names = definitions.Select(d => d.Name).ToHashSet(StringComparer.Ordinal);
        var errors = new HashSet<string>(StringComparer.Ordinal);
        for (int start = text.IndexOf('['); start >= 0; start = text.IndexOf('[', start + 1))
        {
            int end = text.IndexOf(']', start + 1);
            var candidate = end < 0 ? text[(start + 1)..] : text[(start + 1)..end];
            if (!candidate.Contains(':')) continue;
            if (end < 0 || !NamePattern().IsMatch(candidate))
                errors.Add("Invalid template token: " + text.Substring(start, Math.Min((end < 0 ? text.Length : end + 1) - start, 160)));
            else if (!names.Contains(candidate))
                errors.Add("Unknown template token: [" + candidate + "]");
        }
        return errors.OrderBy(e => e, StringComparer.Ordinal).ToArray();
    }

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_-]{0,63}:[A-Za-z0-9_.:-]{1,128}$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();
}
