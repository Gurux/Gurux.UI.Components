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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Text.RegularExpressions;

namespace Gurux.UI.Components.Table;

// Replays only the outer cell templates. Child fragments, component parameters, bindings and
// reference captures retain their original values; Blazor continues to own every DOM node.
#pragma warning disable BL0006 // Render-tree inspection is isolated here and covered by render/lifecycle checks.
/// <summary>Captures outer table cells and replays them in a requested order without rebuilding their child content.</summary>
internal sealed class GXTableCellOrder
{
    /// <summary>Stores the captured render-tree frames, including nested content and component parameters.</summary>
    private readonly RenderTreeFrame[] _frames;
    /// <summary>Stores the outer frame indices and synthetic static markup entries in original order.</summary>
    private readonly List<int> _roots = [];
    /// <summary>Associates each outer frame with its render-tree region identity.</summary>
    private readonly Dictionary<int, string> _scopes = [];
    /// <summary>Stores static HTML fragments separated at outer table-cell boundaries.</summary>
    private readonly Dictionary<int, (string Content, bool Cell, string? Name)> _markup = [];
    /// <summary>Combines render region, column, and original component identity to preserve cell instances when reordered.</summary>
    /// <param name="Scope">The render-tree region identity containing this cell.</param>
    /// <param name="Column">The stable column identifier, when supplied.</param>
    /// <param name="Original">The original render-tree identity to preserve.</param>
    private sealed record CellKey(string Scope, string? Column, object? Original);
    /// <summary>Matches HTML tags and comments to locate static outer table-cell boundaries.</summary>
    private static readonly Regex _tags = new("<!--.*?-->|</?(?<tag>[A-Za-z][A-Za-z0-9:-]*)\\b(?:[^>\"']|\"[^\"]*\"|'[^']*')*>", RegexOptions.Singleline);
    /// <summary>Extracts an HTML element identifier from a static table-cell tag.</summary>
    private static readonly Regex _idAttribute = new("\\s+id\\s*=\\s*(?:\"(?<id>[^\"]*)\"|'(?<id>[^']*)'|(?<id>[^\\s>]+))", RegexOptions.IgnoreCase);

    /// <summary>Captures outer table cells and replays them in a requested order without rebuilding their child content.</summary>
    /// <param name="content">The fragment or HTML content to capture, inspect, or cache.</param>
    internal GXTableCellOrder(RenderFragment content)
    {
        using var capture = new RenderTreeBuilder();
        content(capture);
        var buffer = capture.GetFrames();
        _frames = buffer.Array[..buffer.Count];
        ReadRoots(0, _frames.Length, "");
    }

    /// <summary>Collects outer cells and non-cell content while preserving render-tree region boundaries.</summary>
    /// <param name="start">The first frame index in the render-tree region to inspect.</param>
    /// <param name="end">The exclusive end index of the render-tree region to inspect.</param>
    /// <param name="scope">The stable render-tree region path containing the inspected frames.</param>
    private void ReadRoots(int start, int end, string scope)
    {
        var occurrences = new Dictionary<int, int>();
        for (var index = start; index < end; index += Length(_frames[index]))
        {
            if (_frames[index].FrameType == RenderTreeFrameType.Region)
            {
                int sequence = _frames[index].Sequence;
                int occurrence = occurrences.GetValueOrDefault(sequence);
                occurrences[sequence] = occurrence + 1;
                ReadRoots(index + 1, index + _frames[index].RegionSubtreeLength, scope + "/" + sequence + ":" + occurrence);
            }
            else if (_frames[index].FrameType == RenderTreeFrameType.Markup)
            {
                ReadMarkup(_frames[index].MarkupContent);
            }
            else
            {
                _roots.Add(index);
                _scopes[index] = scope;
            }
        }
    }

    // Razor combines adjacent static cells into one markup frame. Split only their outer
    // boundaries, retaining the original HTML and any nested tables verbatim.
    /// <summary>Separates static HTML at outer table-cell boundaries while retaining nested content verbatim.</summary>
    /// <param name="content">The fragment or HTML content to capture, inspect, or cache.</param>
    private void ReadMarkup(string content)
    {
        int position = 0, start = -1, depth = 0;
        string? cellTag = null, name = null;
        void Add(string value, bool cell, string? id = null)
        {
            if (value.Length == 0)
            {
                return;
            }
            int key = -_markup.Count - 1;
            _markup[key] = (value, cell, id);
            _roots.Add(key);
        }
        foreach (Match tag in _tags.Matches(content))
        {
            string tagName = tag.Groups["tag"].Value.ToLowerInvariant();
            bool closing = tag.Value.StartsWith("</", StringComparison.Ordinal);
            if (start < 0)
            {
                if (closing || tagName is not ("td" or "th"))
                {
                    continue;
                }
                Add(content[position..tag.Index], false);
                start = tag.Index;
                cellTag = tagName;
                depth = 1;
                var id = _idAttribute.Match(tag.Value);
                name = id.Success ? System.Net.WebUtility.HtmlDecode(id.Groups["id"].Value) : null;
            }
            else if (tagName == cellTag)
            {
                depth += closing ? -1 : 1;
                if (depth != 0)
                {
                    continue;
                }
                position = tag.Index + tag.Length;
                Add(content[start..position], true, name);
                start = -1;
            }
        }
        Add(content[(start >= 0 ? start : position)..], false);
    }

    /// <summary>Calculates the number of render-tree frames owned by a frame and its subtree.</summary>
    /// <param name="frame">The render-tree frame whose subtree length is required.</param>
    /// <returns>The frame subtree length, or one for frames without descendants.</returns>
    private static int Length(RenderTreeFrame frame) => frame.FrameType switch
    {
        RenderTreeFrameType.Element => frame.ElementSubtreeLength,
        RenderTreeFrameType.Component => frame.ComponentSubtreeLength,
        RenderTreeFrameType.Region => frame.RegionSubtreeLength,
        _ => 1
    };

    /// <summary>Determines whether an outer entry represents an HTML or Gurux table cell.</summary>
    /// <param name="index">The outer frame or synthetic markup index to inspect or replay.</param>
    /// <returns>True when the entry represents an outer table cell.</returns>
    private bool IsCell(int index)
    {
        if (index < 0)
        {
            return _markup[index].Cell;
        }
        var frame = _frames[index];
        if (frame.FrameType == RenderTreeFrameType.Element)
        {
            return frame.ElementName is "td" or "th";
        }
        if (frame.FrameType != RenderTreeFrameType.Component)
        {
            return false;
        }
        var type = frame.ComponentType;
        return type == typeof(GXTd) || type == typeof(GXFh) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(GXTh<>);
    }

    /// <summary>Reads the explicit column name or element identifier associated with an outer cell.</summary>
    /// <param name="index">The outer frame or synthetic markup index to inspect or replay.</param>
    /// <returns>The explicit column identifier, or null when none exists.</returns>
    private string? Name(int index)
    {
        if (index < 0)
        {
            return _markup[index].Name;
        }
        string? id = null, name = null;
        for (var position = index + 1; position < index + Length(_frames[index]) && _frames[position].FrameType == RenderTreeFrameType.Attribute; ++position)
        {
            var attribute = _frames[position];
            if (attribute.AttributeName.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                id = attribute.AttributeValue as string;
            }
            if (attribute.AttributeName == "Name")
            {
                name = attribute.AttributeValue as string;
            }
            if (attribute.AttributeName == "AdditionalAttributes" && attribute.AttributeValue is IEnumerable<KeyValuePair<string, object>> attributes)
            {
                id ??= attributes.FirstOrDefault(a => a.Key.Equals("id", StringComparison.OrdinalIgnoreCase)).Value as string;
            }
        }
        return string.IsNullOrWhiteSpace(id) ? name : id;
    }

    /// <summary>Gets the column identifiers of outer cells in their original order.</summary>
    internal string?[] Names => _roots.Where(IsCell).Select(Name).ToArray();

    /// <summary>Creates a fragment that replays named cells in the requested order while retaining non-cell content positions.</summary>
    /// <param name="order">The requested ordered list of visible column identifiers.</param>
    /// <param name="headerNames">Optional header identifiers used when body cells have no explicit identifier.</param>
    /// <returns>A fragment preserving the original cell content and requested visible ordering.</returns>
    internal RenderFragment Render(string[]? order, string?[]? headerNames = null) => builder =>
    {
        var names = new Dictionary<int, string>();
        int cell = 0;
        foreach (var root in _roots.Where(IsCell))
        {
            var name = Name(root) ?? (headerNames != null && cell < headerNames.Length ? headerNames[cell] : null);
            if (!string.IsNullOrWhiteSpace(name))
            {
                names[root] = name;
            }
            ++cell;
        }
        var positions = (order ?? []).Select((name, index) => (name, index))
            .DistinctBy(pair => pair.name, StringComparer.Ordinal).ToDictionary(pair => pair.name, pair => pair.index, StringComparer.Ordinal);
        var sorted = _roots.Where(names.ContainsKey).OrderBy(root => positions.GetValueOrDefault(names[root], int.MaxValue)).ToArray();
        int next = 0, sequence = 0;
        foreach (var source in _roots)
        {
            var root = names.ContainsKey(source) ? sorted[next++] : source;
            Replay(builder, root, sequence++, names.GetValueOrDefault(root));
        }
    };

    /// <summary>Copies an outer frame and its descendants into a target render tree while preserving bindings and references.</summary>
    /// <param name="builder">The target render-tree builder receiving the captured frames.</param>
    /// <param name="index">The outer frame or synthetic markup index to inspect or replay.</param>
    /// <param name="sequence">The optional sequence number replacing the original outer frame sequence.</param>
    /// <param name="columnKey">The stable column identifier used to preserve component identity during reordering.</param>
    /// <returns>The first frame index after the replayed subtree.</returns>
    private int Replay(RenderTreeBuilder builder, int index, int? sequence = null, string? columnKey = null)
    {
        if (index < 0)
        {
            builder.AddMarkupContent(sequence!.Value, _markup[index].Content);
            return index + 1;
        }
        var frame = _frames[index];
        int seq = sequence ?? frame.Sequence;
        int end = index + Length(frame);
        switch (frame.FrameType)
        {
            case RenderTreeFrameType.Element:
                builder.OpenElement(seq, frame.ElementName);
                if (columnKey != null || frame.ElementKey != null)
                {
                    builder.SetKey(sequence.HasValue ? new CellKey(_scopes[index], columnKey, frame.ElementKey) : frame.ElementKey);
                }
                for (int child = index + 1; child < end;)
                {
                    child = Replay(builder, child);
                }
                builder.CloseElement();
                break;
            case RenderTreeFrameType.Component:
                builder.OpenComponent(seq, frame.ComponentType);
                if (columnKey != null || frame.ComponentKey != null)
                {
                    builder.SetKey(sequence.HasValue ? new CellKey(_scopes[index], columnKey, frame.ComponentKey) : frame.ComponentKey);
                }
                for (int child = index + 1; child < end;)
                {
                    child = Replay(builder, child);
                }
                builder.CloseComponent();
                break;
            case RenderTreeFrameType.Region:
                builder.OpenRegion(seq);
                for (int child = index + 1; child < end;)
                {
                    child = Replay(builder, child);
                }
                builder.CloseRegion();
                break;
            case RenderTreeFrameType.Attribute:
                builder.AddAttribute(seq, frame.AttributeName, frame.AttributeValue);
                if (frame.AttributeEventUpdatesAttributeName != null)
                {
                    builder.SetUpdatesAttributeName(frame.AttributeEventUpdatesAttributeName);
                }
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
