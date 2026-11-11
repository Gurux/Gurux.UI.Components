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

using System.Net;
using System.Text;

namespace Gurux.UI.Components.Help;

/// <summary>Retrieves original HTML documents with bounded transport and caching.</summary>
/// <remarks>Configure HTTP handlers to disable automatic redirects. Supplied clients remain caller-owned.</remarks>
public sealed class GXHelpArticleReader : IDisposable
{
    private const int MaximumBytes = 2 * 1024 * 1024;
    private readonly HttpClient? _http;
    private readonly Func<HttpClient>? _createClient;
    private readonly Func<Uri, bool> _allowedAddress;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<Uri, (DateTimeOffset Expires, string Content)> _cache = new();
    private bool _disposed;

    /// <summary>Creates a reader using a caller-owned client, an address allowlist.</summary>
    public GXHelpArticleReader(HttpClient http, Func<Uri, bool> allowedAddress)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _allowedAddress = allowedAddress ?? throw new ArgumentNullException(nameof(allowedAddress));
    }

    /// <summary>Creates a reader that disposes each factory-created client after fetching an article.</summary>
    public GXHelpArticleReader(Func<HttpClient> createClient, Func<Uri, bool> allowedAddress)
    {
        _createClient = createClient ?? throw new ArgumentNullException(nameof(createClient));
        _allowedAddress = allowedAddress ?? throw new ArgumentNullException(nameof(allowedAddress));
    }

    /// <summary>Returns unmodified HTML. The host must sanitize the document before displaying it.</summary>
    public Task<string?> LoadHtmlAsync(Uri address, CancellationToken cancellationToken = default) => LoadHtmlAsync(address, null, cancellationToken);

    /// <summary>Loads unmodified HTML, using a separately allowed fallback when the server returns 404.</summary>
    public Task<string?> LoadHtmlAsync(Uri address, Uri? fallbackAddress, CancellationToken cancellationToken = default) => LoadCoreAsync(address, fallbackAddress, cancellationToken);

    private async Task<string?> LoadCoreAsync(Uri address, Uri? fallbackAddress, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Validate(address);
        if (fallbackAddress != null) { Validate(fallbackAddress); }
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        var token = deadline.Token;
        await _gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            token.ThrowIfCancellationRequested();
            var now = DateTimeOffset.UtcNow;
            foreach (var key in _cache.Where(item => item.Value.Expires <= now).Select(item => item.Key).ToArray())
            {
                _cache.Remove(key);
            }
            if (_cache.TryGetValue(address, out var cached)) { return cached.Content; }
            var content = await ReadAsync(address, token).ConfigureAwait(false);
            if (content == null && fallbackAddress != null && fallbackAddress != address)
            {
                if (_cache.TryGetValue(fallbackAddress, out var fallbackCached)) { content = fallbackCached.Content; }
                else
                {
                    content = await ReadAsync(fallbackAddress, token).ConfigureAwait(false);
                    if (content != null)
                    {
                        Cache(fallbackAddress, content);
                    }
                }
            }
            if (content != null)
            {
                Cache(address, content);
            }
            return content;
        }
        finally { _gate.Release(); }
    }

    private void Cache(Uri address, string content)
    {
        if (_cache.Count >= 128 && !_cache.ContainsKey(address))
        {
            _cache.Remove(_cache.MinBy(item => item.Value.Expires).Key);
        }
        _cache[address] = (DateTimeOffset.UtcNow.AddHours(1), content);
    }

    private void Validate(Uri address)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (!address.IsAbsoluteUri || address.Scheme is not ("http" or "https") || address.UserInfo.Length != 0 || !_allowedAddress(address))
        {
            throw new ArgumentException("The help article address is not allowed.", nameof(address));
        }
    }

    private async Task<string?> ReadAsync(Uri address, CancellationToken token)
    {
        var http = _http ?? _createClient!();
        try
        {
            using var response = await http.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
            var finalAddress = response.RequestMessage?.RequestUri ?? address;
            Validate(finalAddress);
            if (response.StatusCode == HttpStatusCode.NotFound) { return null; }
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > MaximumBytes)
            {
                throw new InvalidDataException("The help article is too large.");
            }
            await using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
            using var buffer = new MemoryStream();
            var bytes = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(bytes, token).ConfigureAwait(false)) != 0)
            {
                if (buffer.Length + count > MaximumBytes) { throw new InvalidDataException("The help article is too large."); }
                await buffer.WriteAsync(bytes.AsMemory(0, count), token).ConfigureAwait(false);
            }
            var source = Encoding.UTF8.GetString(buffer.ToArray());
            token.ThrowIfCancellationRequested();
            return source;
        }
        finally
        {
            if (_createClient != null) { http.Dispose(); }
        }
    }

    public void Dispose()
    {
        // Do not dispose the semaphore while active requests may still release it.
        _disposed = true;
    }

}
