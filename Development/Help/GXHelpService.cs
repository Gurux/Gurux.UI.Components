//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using System.Collections.ObjectModel;

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
