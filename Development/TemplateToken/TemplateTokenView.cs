using System.Text;
using System.Text.RegularExpressions;

namespace Gurux.UI.Components.TemplateToken;

/// <summary>A plain-text editor view with an explicit indication of example substitution.</summary>
public sealed record TemplateTokenViewResult(string Text, bool UsesExamples);

/// <summary>Creates an editor view without recursively expanding replacement values.</summary>
public static partial class TemplateTokenView
{
    /// <summary>Resolves original token references once, optionally using example values for unavailable tokens.</summary>
    public static async Task<TemplateTokenViewResult> CreateAsync(string text,
        IEnumerable<TemplateTokenDefinition> definitions, Func<string, CancellationToken, Task<string?>> resolve,
        bool allowExamples, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(resolve);
        cancellationToken.ThrowIfCancellationRequested();
        var examples = definitions.GroupBy(d => d.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().ExampleValue, StringComparer.Ordinal);
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
        var result = new StringBuilder();
        int position = 0;
        bool usesExamples = false;
        foreach (Match match in TokenPattern().Matches(text))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!replacements.TryGetValue(match.Value, out var value))
            {
                value = await resolve(match.Value, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (value == null || value == match.Value)
                {
                    if (allowExamples && examples.TryGetValue(match.Groups[1].Value, out var example) && example != null)
                    { value = example; usesExamples = true; }
                    else value = match.Value;
                }
                replacements[match.Value] = value;
            }
            result.Append(text, position, match.Index - position);
            result.Append(value);
            position = match.Index + match.Length;
        }
        result.Append(text, position, text.Length - position);
        return new(result.ToString(), usesExamples);
    }

    [GeneratedRegex(@"\[([A-Za-z][A-Za-z0-9_-]{0,63}:[A-Za-z0-9_.:-]{1,128})\]", RegexOptions.CultureInvariant)]
    private static partial Regex TokenPattern();
}
