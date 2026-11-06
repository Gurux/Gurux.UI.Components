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
    /// Resolves contextual help requests, loads provider content and controls where the help icon is shown.
    /// </summary>
    public sealed class GXHelpService
    {
        private readonly List<(object Owner, GXHelpRequest Request)> _contexts = new();
        private readonly IReadOnlyList<IGXHelpProvider> _providers;
        private GXHelpRequest? _defaultRequest;

        /// <summary>
        /// Registers or updates an owner's complete contextual request and resolves the active request.
        /// </summary>
        internal void SetContext(object owner, GXHelpRequest request)
        {
            int index = _contexts.FindIndex(context => ReferenceEquals(context.Owner, owner));
            if (index < 0)
            {
                _contexts.Add((owner, request));
            }
            else
            {
                _contexts[index] = (owner, request);
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
        /// Selects the most recently registered request or the default and notifies listeners when any request field changes.
        /// </summary>
        private void RefreshTopic()
        {
            var request = _contexts.Count == 0 ? _defaultRequest : _contexts[^1].Request;
            if (Request == request)
            {
                return;
            }
            Request = request;
            Changed?.Invoke();
        }

        /// <summary>
        /// Creates a help service with no registered contexts or default topic.
        /// </summary>
        public GXHelpService() : this(Array.Empty<IGXHelpProvider>())
        {
        }

        /// <summary>Creates a help service using providers in their registration order.</summary>
        /// <param name="providers">Providers registered for the current application scope.</param>
        public GXHelpService(IEnumerable<IGXHelpProvider> providers)
        {
            ArgumentNullException.ThrowIfNull(providers);
            _providers = providers.ToArray();
        }

        /// <summary>Loads help for a snapshot of the active request, or returns null when no topic is active.</summary>
        /// <param name="cancellationToken">Token used to cancel loading.</param>
        /// <returns>The first matching provider's content, or null when no provider handles the request.</returns>
        public Task<GXHelpContent?> GetHelpAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = Request;
            return request is null ? Task.FromResult<GXHelpContent?>(null) : GetHelpAsync(request, cancellationToken);
        }

        /// <summary>Loads a specific help request without changing the active context.</summary>
        /// <param name="request">The topic, subject version and culture to resolve.</param>
        /// <param name="cancellationToken">Token used to cancel loading.</param>
        /// <returns>The first non-null provider result, or null for an empty or unsupported topic.</returns>
        /// <remarks>Providers run in registration order. Errors and cancellation propagate; content is not cached by this service.</remarks>
        public async Task<GXHelpContent?> GetHelpAsync(GXHelpRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                return null;
            }
            foreach (var provider in _providers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var content = await provider.GetHelpAsync(request, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (content is not null)
                {
                    return content;
                }
            }
            return null;
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
        /// Occurs when the active topic, subject version or culture changes.
        /// </summary>
        public event Action? Changed;

        /// <summary>
        /// Gets the active help topic resolved from registered contexts or the default topic.
        /// </summary>
        public string? Topic => Request?.Topic;

        /// <summary>Gets the active immutable request, including the subject version and culture.</summary>
        public GXHelpRequest? Request { get; private set; }

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
            SetRequest(topic is null ? null : new GXHelpRequest(topic));
        }

        /// <summary>Sets the default request while preserving any registered component contexts.</summary>
        /// <param name="request">Default help request, or null to clear it.</param>
        public void SetRequest(GXHelpRequest? request)
        {
            _defaultRequest = request;
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
