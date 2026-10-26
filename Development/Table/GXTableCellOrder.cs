using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Text.RegularExpressions;

namespace Gurux.UI.Components.Table;

// Replays only the outer cell templates. Child fragments, component parameters, bindings and
// reference captures retain their original values; Blazor continues to own every DOM node.
#pragma warning disable BL0006 // Render-tree inspection is isolated here and covered by render/lifecycle checks.
internal sealed class GXTableCellOrder
{
    private readonly RenderTreeFrame[] frames;
    private readonly List<int> roots = [];
    private readonly Dictionary<int, string> scopes = [];
    private readonly Dictionary<int, (string Content, bool Cell, string? Name)> markup = [];
    private sealed record CellKey(string Scope, string? Column, object? Original);
    private static readonly Regex Tags = new("<!--.*?-->|</?(?<tag>[A-Za-z][A-Za-z0-9:-]*)\\b(?:[^>\"']|\"[^\"]*\"|'[^']*')*>", RegexOptions.Singleline);
    private static readonly Regex IdAttribute = new("\\s+id\\s*=\\s*(?:\"(?<id>[^\"]*)\"|'(?<id>[^']*)'|(?<id>[^\\s>]+))", RegexOptions.IgnoreCase);

    internal GXTableCellOrder(RenderFragment content)
    {
        using var capture = new RenderTreeBuilder();
        content(capture);
        var buffer = capture.GetFrames();
        frames = buffer.Array[..buffer.Count];
        ReadRoots(0, frames.Length, "");
    }

    private void ReadRoots(int start, int end, string scope)
    {
        var occurrences = new Dictionary<int, int>();
        for (var index = start; index < end; index += Length(frames[index]))
        {
            if (frames[index].FrameType == RenderTreeFrameType.Region)
            {
                int sequence = frames[index].Sequence;
                int occurrence = occurrences.GetValueOrDefault(sequence);
                occurrences[sequence] = occurrence + 1;
                ReadRoots(index + 1, index + frames[index].RegionSubtreeLength, scope + "/" + sequence + ":" + occurrence);
            }
            else if (frames[index].FrameType == RenderTreeFrameType.Markup)
                ReadMarkup(frames[index].MarkupContent);
            else
            {
                roots.Add(index);
                scopes[index] = scope;
            }
        }
    }

    // Razor combines adjacent static cells into one markup frame. Split only their outer
    // boundaries, retaining the original HTML and any nested tables verbatim.
    private void ReadMarkup(string content)
    {
        int position = 0, start = -1, depth = 0;
        string? cellTag = null, name = null;
        void Add(string value, bool cell, string? id = null)
        {
            if (value.Length == 0) return;
            int key = -markup.Count - 1;
            markup[key] = (value, cell, id);
            roots.Add(key);
        }
        foreach (Match tag in Tags.Matches(content))
        {
            string tagName = tag.Groups["tag"].Value.ToLowerInvariant();
            bool closing = tag.Value.StartsWith("</", StringComparison.Ordinal);
            if (start < 0)
            {
                if (closing || tagName is not ("td" or "th")) continue;
                Add(content[position..tag.Index], false);
                start = tag.Index;
                cellTag = tagName;
                depth = 1;
                var id = IdAttribute.Match(tag.Value);
                name = id.Success ? System.Net.WebUtility.HtmlDecode(id.Groups["id"].Value) : null;
            }
            else if (tagName == cellTag)
            {
                depth += closing ? -1 : 1;
                if (depth != 0) continue;
                position = tag.Index + tag.Length;
                Add(content[start..position], true, name);
                start = -1;
            }
        }
        Add(content[(start >= 0 ? start : position)..], false);
    }

    private static int Length(RenderTreeFrame frame) => frame.FrameType switch
    {
        RenderTreeFrameType.Element => frame.ElementSubtreeLength,
        RenderTreeFrameType.Component => frame.ComponentSubtreeLength,
        RenderTreeFrameType.Region => frame.RegionSubtreeLength,
        _ => 1
    };

    private bool IsCell(int index)
    {
        if (index < 0) return markup[index].Cell;
        var frame = frames[index];
        if (frame.FrameType == RenderTreeFrameType.Element) return frame.ElementName is "td" or "th";
        if (frame.FrameType != RenderTreeFrameType.Component) return false;
        var type = frame.ComponentType;
        return type == typeof(Td) || type == typeof(Fh) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Th<>);
    }

    private string? Name(int index)
    {
        if (index < 0) return markup[index].Name;
        string? id = null, name = null;
        for (var position = index + 1; position < index + Length(frames[index]) && frames[position].FrameType == RenderTreeFrameType.Attribute; ++position)
        {
            var attribute = frames[position];
            if (attribute.AttributeName.Equals("Id", StringComparison.OrdinalIgnoreCase)) id = attribute.AttributeValue as string;
            if (attribute.AttributeName == "Name") name = attribute.AttributeValue as string;
            if (attribute.AttributeName == "AdditionalAttributes" && attribute.AttributeValue is IEnumerable<KeyValuePair<string, object>> attributes)
                id ??= attributes.FirstOrDefault(a => a.Key.Equals("id", StringComparison.OrdinalIgnoreCase)).Value as string;
        }
        return string.IsNullOrWhiteSpace(id) ? name : id;
    }

    internal string?[] Names => roots.Where(IsCell).Select(Name).ToArray();

    internal RenderFragment Render(string[]? order, string?[]? headerNames = null) => builder =>
    {
        var names = new Dictionary<int, string>();
        int cell = 0;
        foreach (var root in roots.Where(IsCell))
        {
            var name = Name(root) ?? (headerNames != null && cell < headerNames.Length ? headerNames[cell] : null);
            if (!string.IsNullOrWhiteSpace(name)) names[root] = name;
            ++cell;
        }
        var positions = (order ?? []).Select((name, index) => (name, index))
            .DistinctBy(pair => pair.name, StringComparer.Ordinal).ToDictionary(pair => pair.name, pair => pair.index, StringComparer.Ordinal);
        var sorted = roots.Where(names.ContainsKey).OrderBy(root => positions.GetValueOrDefault(names[root], int.MaxValue)).ToArray();
        int next = 0, sequence = 0;
        foreach (var source in roots)
        {
            var root = names.ContainsKey(source) ? sorted[next++] : source;
            Replay(builder, root, sequence++, names.GetValueOrDefault(root));
        }
    };

    private int Replay(RenderTreeBuilder builder, int index, int? sequence = null, string? columnKey = null)
    {
        if (index < 0)
        {
            builder.AddMarkupContent(sequence!.Value, markup[index].Content);
            return index + 1;
        }
        var frame = frames[index];
        int seq = sequence ?? frame.Sequence;
        int end = index + Length(frame);
        switch (frame.FrameType)
        {
            case RenderTreeFrameType.Element:
                builder.OpenElement(seq, frame.ElementName);
                if (columnKey != null || frame.ElementKey != null)
                    builder.SetKey(sequence.HasValue ? new CellKey(scopes[index], columnKey, frame.ElementKey) : frame.ElementKey);
                for (int child = index + 1; child < end;) child = Replay(builder, child);
                builder.CloseElement();
                break;
            case RenderTreeFrameType.Component:
                builder.OpenComponent(seq, frame.ComponentType);
                if (columnKey != null || frame.ComponentKey != null)
                    builder.SetKey(sequence.HasValue ? new CellKey(scopes[index], columnKey, frame.ComponentKey) : frame.ComponentKey);
                for (int child = index + 1; child < end;) child = Replay(builder, child);
                builder.CloseComponent();
                break;
            case RenderTreeFrameType.Region:
                builder.OpenRegion(seq);
                for (int child = index + 1; child < end;) child = Replay(builder, child);
                builder.CloseRegion();
                break;
            case RenderTreeFrameType.Attribute:
                builder.AddAttribute(seq, frame.AttributeName, frame.AttributeValue);
                if (frame.AttributeEventUpdatesAttributeName != null) builder.SetUpdatesAttributeName(frame.AttributeEventUpdatesAttributeName);
                break;
            case RenderTreeFrameType.Text: builder.AddContent(seq, frame.TextContent); break;
            case RenderTreeFrameType.Markup: builder.AddMarkupContent(seq, frame.MarkupContent); break;
            case RenderTreeFrameType.ElementReferenceCapture: builder.AddElementReferenceCapture(seq, frame.ElementReferenceCaptureAction); break;
            case RenderTreeFrameType.ComponentReferenceCapture: builder.AddComponentReferenceCapture(seq, frame.ComponentReferenceCaptureAction); break;
            case RenderTreeFrameType.ComponentRenderMode: builder.AddComponentRenderMode(frame.ComponentRenderMode); break;
            case RenderTreeFrameType.NamedEvent: builder.AddNamedEvent(frame.NamedEventType, frame.NamedEventAssignedName); break;
            default: throw new NotSupportedException($"Unsupported table cell render frame: {frame.FrameType}.");
        }
        return end;
    }
}
#pragma warning restore BL0006
