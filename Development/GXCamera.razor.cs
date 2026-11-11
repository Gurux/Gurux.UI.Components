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

using Gurux.UI.Components.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gurux.UI.Components;

/// <summary>Captures camera images using an isolated browser controller and binary stream interop.</summary>
public partial class GXCamera
{
    /// <summary>Gets or sets the requested encoding. The actual encoding is returned with the result.</summary>
    [Parameter] public ImageFormat ImageFormat { get; set; } = ImageFormat.Jpeg;
    /// <summary>Gets or sets the positive maximum image width in pixels, preserving aspect ratio.</summary>
    [Parameter] public int MaxWidth { get; set; } = 1920;
    /// <summary>Gets or sets the positive maximum image height in pixels, preserving aspect ratio.</summary>
    [Parameter] public int MaxHeight { get; set; } = 1080;
    /// <summary>Gets or sets JPEG/WebP quality, from zero to one. PNG ignores quality.</summary>
    [Parameter] public double Quality { get; set; } = 0.85;
    /// <summary>Gets or sets the preferred initial camera direction.</summary>
    [Parameter] public CameraFacingMode FacingMode { get; set; } = CameraFacingMode.Environment;
    /// <summary>Gets or sets whether a local object-URL preview is shown after capture.</summary>
    [Parameter] public bool ShowPreview { get; set; } = true;
    /// <summary>Gets or sets whether camera permission is requested after the first interactive render.</summary>
    [Parameter] public bool AutoStart { get; set; }
    /// <summary>Gets or sets the callback receiving a captured image. No image is stored automatically.</summary>
    [Parameter] public EventCallback<GXCameraResult> OnPhotoTaken { get; set; }
    /// <summary>Gets or sets the callback receiving browser, permission and interop error messages.</summary>
    [Parameter] public EventCallback<string> OnError { get; set; }
    /// <summary>Gets or sets the accessible captured-image description.</summary>
    [Parameter] public string PreviewAltText { get; set; } = "Captured photo";

    [Inject] private IJSRuntime JS { get; set; } = default!;
    private ElementReference _video;
    private ElementReference _preview;
    private GXBrowserDeviceSession? _session;
    private readonly CancellationTokenSource _lifetime = new();
    private bool _rendered;
    private bool _disposed;
    private bool _running;
    private bool _busy;
    private bool _capturing;
    private bool _hasPhoto;
    private int _revision;
    private string? _error;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (MaxWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxWidth));
        }
        if (MaxHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxHeight));
        }
        if (!double.IsFinite(Quality) || Quality is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(Quality));
        }
        if (!Enum.IsDefined(ImageFormat))
        {
            throw new ArgumentOutOfRangeException(nameof(ImageFormat));
        }
        if (!Enum.IsDefined(FacingMode))
        {
            throw new ArgumentOutOfRangeException(nameof(FacingMode));
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _rendered = true;
        if (firstRender && AutoStart && !_disposed)
        {
            await StartCameraAsync();
        }
    }

    private Task<IJSObjectReference> ControllerAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_rendered)
        {
            throw new InvalidOperationException("Camera methods require an interactive render.");
        }
        _session ??= new GXBrowserDeviceSession(JS, "./_content/Gurux.UI.Components/js/gx-camera.js", _video, _preview);
        return _session.GetAsync();
    }

    /// <summary>Requests camera permission and starts the preview. Browser failures are reported through OnError.</summary>
    public Task StartCameraAsync() => ChangeCameraAsync(false);

    /// <summary>Cycles exposed camera devices, falling back to toggling the preferred facing mode.</summary>
    public Task SwitchCameraAsync() => ChangeCameraAsync(true);

    private async Task ChangeCameraAsync(bool switchCamera)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var revision = ++_revision;
        _busy = true;
        try
        {
            var controller = await ControllerAsync();
            if (_disposed || revision != _revision)
            {
                return;
            }
            var running = await controller.InvokeAsync<bool>(switchCamera ? "switchCamera" : "start",
                _lifetime.Token, FacingMode == CameraFacingMode.Environment ? "environment" : "user");
            if (!_disposed && revision == _revision)
            {
                _running = running;
                _hasPhoto = false;
                _error = null;
            }
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error) || (_disposed && error is ObjectDisposedException))
        {
            if (!_disposed && revision == _revision)
            {
                _running = false;
                await ReportAsync(error);
            }
        }
        finally
        {
            if (!_disposed && revision == _revision)
            {
                _busy = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    /// <summary>Stops only this component's stream, including any pending startup.</summary>
    public async Task StopCameraAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ++_revision;
        _running = false;
        _busy = false;
        try
        {
            if (_session is not null)
            {
                var controller = await _session.GetAsync();
                if (!_disposed)
                {
                    await controller.InvokeVoidAsync("stop");
                }
            }
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error) || (_disposed && error is ObjectDisposedException))
        {
            await ReportAsync(error);
        }
        if (!_disposed)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>Captures an image and invokes OnPhotoTaken. Reports browser errors through OnError and rethrows them.</summary>
    /// <exception cref="InvalidOperationException">The component is not rendered or another capture is active.</exception>
    public async Task<GXCameraResult> TakePhotoAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        OnParametersSet();
        if (_capturing)
        {
            throw new InvalidOperationException("A photo capture is already in progress.");
        }
        _capturing = true;
        _busy = true;
        var revision = _revision;
        GXCameraResult result;
        try
        {
            var controller = await ControllerAsync();
            var contentType = ImageFormat switch
            {
                ImageFormat.Png => "image/png",
                ImageFormat.Jpeg => "image/jpeg",
                ImageFormat.WebP => "image/webp",
                _ => throw new ArgumentOutOfRangeException(nameof(ImageFormat))
            };
            // Receive ownership even if disposal races with the returned JS stream reference.
            // The controller cancels pending encoding; any returned reference is disposed below.
            var capture = await controller.InvokeAsync<CameraCapture>("capture", CancellationToken.None, new
            {
                contentType, maxWidth = MaxWidth, maxHeight = MaxHeight, quality = Quality, showPreview = ShowPreview
            });
            await using var reference = capture.Stream;
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (revision != _revision)
            {
                throw new OperationCanceledException("The camera capture was canceled.");
            }
            var extension = capture.ContentType switch
            {
                "image/png" => "png",
                "image/jpeg" => "jpg",
                "image/webp" => "webp",
                _ => throw new JSException("The browser returned an unsupported image encoding.")
            };
            await using var input = await reference.OpenReadStreamAsync(reference.Length, _lifetime.Token);
            using var output = new MemoryStream();
            await input.CopyToAsync(output, _lifetime.Token);
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (revision != _revision)
            {
                throw new OperationCanceledException("The camera capture was canceled.");
            }
            result = new GXCameraResult
            {
                Image = output.ToArray(), ContentType = capture.ContentType,
                FileName = $"camera-{capture.Timestamp:yyyyMMdd-HHmmss-fff}.{extension}",
                Width = capture.Width, Height = capture.Height, Timestamp = capture.Timestamp
            };
            _hasPhoto = ShowPreview;
            _error = null;
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error) || error is IOException)
        {
            await ReportAsync(error);
            throw;
        }
        finally
        {
            _capturing = false;
            if (!_disposed && revision == _revision)
            {
                _busy = false;
                await InvokeAsync(StateHasChanged);
            }
        }
        ObjectDisposedException.ThrowIf(_disposed, this);
        await OnPhotoTaken.InvokeAsync(result);
        return result;
    }

    /// <summary>Clears the captured preview and returns to the live view, starting the camera if necessary.</summary>
    public async Task RetakeAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            var controller = await ControllerAsync();
            if (_disposed)
            {
                return;
            }
            await controller.InvokeVoidAsync("retake");
            _hasPhoto = false;
            if (!_running)
            {
                await StartCameraAsync();
            }
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error) || (_disposed && error is ObjectDisposedException))
        {
            await ReportAsync(error);
        }
        if (!_disposed)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CaptureFromButtonAsync()
    {
        try
        {
            await TakePhotoAsync();
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error) || error is IOException || (_disposed && error is ObjectDisposedException)) { }
    }

    private async Task ReportAsync(Exception error)
    {
        if (!_disposed)
        {
            _error = error.Message;
            await OnError.InvokeAsync(_error);
        }
    }

    /// <summary>Releases this component's stream, preview URL, module and pending binary transfer.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        ++_revision;
        _lifetime.Cancel();
        if (_session is not null)
        {
            await _session.DisposeAsync();
        }
        _lifetime.Dispose();
    }

    internal sealed class CameraCapture
    {
        public CameraCapture() { }
        public IJSStreamReference Stream { get; set; } = default!;
        public string ContentType { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
