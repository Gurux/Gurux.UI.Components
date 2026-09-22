namespace Gurux.UI.Components;

/// <summary>
/// Ends a progress operation once when its scope is disposed.
/// </summary>
public sealed class GXProgressScope : IDisposable
{
    private Action? _end;
    private readonly CancellationTokenSource _cancellation = new();
    public CancellationToken CancellationToken => _cancellation.Token;
    public Task CancelAsync()
    {
        if (Volatile.Read(ref _end) is null) return Task.CompletedTask;
        try { return _cancellation.CancelAsync(); }
        catch (ObjectDisposedException) { return Task.CompletedTask; }
    }
    public Guid Id { get; } = Guid.NewGuid();
    public GXProgressScope(Action<Guid> end)
    {
        ArgumentNullException.ThrowIfNull(end);
        _end = () => end(Id);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GXProgressScope"/> class.
    /// </summary>
    /// <param name="end">The action to invoke when the scope is disposed.</param>
    public GXProgressScope(Action end)
    {
        ArgumentNullException.ThrowIfNull(end);
        _end = end;
    }

    /// <summary>
    /// Disposes the scope and invokes the end action if it hasn't been invoked yet.
    /// </summary>
    public void Dispose()
    {
        Action? end = Interlocked.Exchange(ref _end, null);
        if (end == null) return;
        try { end(); }
        finally { _cancellation.Dispose(); }
    }
}


