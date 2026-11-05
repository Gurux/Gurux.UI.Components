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

namespace Gurux.UI.Components.Help
{
    /// <summary>
    /// Resolves the active help topic from registered contexts and controls where the help icon is shown.
    /// </summary>
    public sealed class GXHelpService
    {
        private readonly List<(object Owner, string Topic)> _contexts = new();
        private string? _defaultTopic;

        /// <summary>
        /// Registers or updates an owner's contextual help topic and resolves the active topic.
        /// </summary>
        internal void SetContext(object owner, string topic)
        {
            int index = _contexts.FindIndex(context => ReferenceEquals(context.Owner, owner));
            if (index < 0)
            {
                _contexts.Add((owner, topic));
            }
            else
            {
                _contexts[index] = (owner, topic);
            }
            RefreshTopic();
        }

        /// <summary>
        /// Removes an owner's contextual help topic and restores the next active topic.
        /// </summary>
        internal void RemoveContext(object owner)
        {
            _contexts.RemoveAll(context => ReferenceEquals(context.Owner, owner));
            RefreshTopic();
        }

        /// <summary>
        /// Selects the most recently registered context topic or the default and notifies listeners when it changes.
        /// </summary>
        private void RefreshTopic()
        {
            var topic = _contexts.Count == 0 ? _defaultTopic : _contexts[^1].Topic;
            if (Topic == topic)
            {
                return;
            }
            Topic = topic;
            Changed?.Invoke();
        }

        /// <summary>
        /// Creates a help service with no registered contexts or default topic.
        /// </summary>
        public GXHelpService()
        {
        }

        /// <summary>
        /// Gets or sets absolute URLs or origin-relative paths on which the help icon is hidden.
        /// </summary>
        public IEnumerable<string>? HiddenUrls { get; set; }

        /// <summary>
        /// Matches exact page paths while ignoring query, fragment, and trailing slash; absolute entries must also match the origin.
        /// </summary>
        public bool IsHidden(string currentUrl)
        {
            if (!Uri.TryCreate(currentUrl, UriKind.Absolute, out var current) || current.Scheme is not ("http" or "https"))
            {
                return false;
            }

            foreach (var entry in HiddenUrls ?? [])
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }
                var value = entry.Trim();
                Uri? hidden;
                if (value.StartsWith('/') && !value.StartsWith("//"))
                {
                    if (!Uri.TryCreate(new Uri(current.GetLeftPart(UriPartial.Authority)), value, out hidden))
                    {
                        continue;
                    }
                }
                else if (!Uri.TryCreate(value, UriKind.Absolute, out hidden))
                {
                    continue;
                }

                if (hidden.Scheme is not ("http" or "https"))
                {
                    continue;
                }
                if (string.Equals(current.GetLeftPart(UriPartial.Authority), hidden.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase) && string.Equals(current.AbsolutePath.TrimEnd('/'), hidden.AbsolutePath.TrimEnd('/'), StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets or sets the image of the help icon.
        /// </summary>
        public string? Image;

        /// <summary>
        /// Gets or sets the base URL of the help documentation.
        /// </summary>
        public string BaseAddress { get; set; } = "/";

        /// <summary>
        /// Occurs when resolving registered contexts or the default topic changes the active help topic.
        /// </summary>
        public event Action? Changed;

        /// <summary>
        /// Gets the active help topic resolved from registered contexts or the default topic.
        /// </summary>
        public string? Topic { get; private set; }

        /// <summary>
        /// Gets the documentation URL with the current topic appended as a fragment.
        /// </summary>
        public string HelpUrl =>
            string.IsNullOrEmpty(Topic)
                ? BaseAddress
                : $"{BaseAddress}#{Topic}";

        /// <summary>
        /// Sets the default help topic used when no contextual topic is registered.
        /// </summary>
        /// <param name="topic">Help topic anchor.</param>
        public void SetTopic(string? topic)
        {
            _defaultTopic = topic;
            RefreshTopic();
        }

        /// <summary>
        /// Clears the default help topic while preserving any registered contextual topics.
        /// </summary>
        public void Clear()
        {
            SetTopic(null);
        }
    }
}
