namespace Gurux.UI.Components;

/// <summary>
/// Tracks active progress operations and their messages within one application scope.
/// </summary>
public sealed class GXProgressService : IGXProgress
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, (GXProgressScope Scope, string? Message)> _operations = new();

    /// <summary>
    /// Occurs when progress operations are added or ended.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Gets whether at least one progress operation is active, including operations without a message.
    /// </summary>
    public bool IsBusy
    {
        get { lock (_sync) return _operations.Count != 0; }
    }

    /// <summary>
    /// Gets the nonempty messages of the currently active progress operations.
    /// </summary>
    public IReadOnlyList<string> Tasks
    {
        get
        {
            lock (_sync)
                return _operations.Values.Select(operation => operation.Message)
                    .Where(message => !string.IsNullOrWhiteSpace(message)).Select(message => message!).ToArray();
        }
    }

    /// <summary>
    /// Registers a progress operation and returns a scope whose disposal ends that operation.
    /// </summary>
    public GXProgressScope ProgressStart(string? message)
    {
        var scope = new GXProgressScope(ProgressEnd);
        lock (_sync) _operations.Add(scope.Id, (scope, message));
        Changed?.Invoke();
        return scope;
    }

    /// <summary>
    /// Ends the progress operation with the specified id and notifies listeners when an operation was removed.
    /// </summary>
    public void ProgressEnd(Guid id)
    {
        GXProgressScope scope;
        lock (_sync)
        {
            if (!_operations.Remove(id, out var operation))
            {
                return;
            }
            scope = operation.Scope;
        }
        scope.Dispose();
        Changed?.Invoke();
    }

    /// <summary>
    /// Requests cancellation of all currently active progress operations.
    /// </summary>
    public Task CancelAllAsync()
    {
        GXProgressScope[] scopes;
        lock (_sync) scopes = _operations.Values.Select(operation => operation.Scope).ToArray();
        return Task.WhenAll(scopes.Select(scope => scope.CancelAsync()));
    }
}
