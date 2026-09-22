namespace Gurux.UI.Components;

/// <summary>Shared progress state for one application scope.</summary>
public sealed class GXProgressService : IGXProgress
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, (GXProgressScope Scope, string? Message)> _operations = new();

    public event Action? Changed;

    public bool IsBusy
    {
        get { lock (_sync) return _operations.Count != 0; }
    }

    public IReadOnlyList<string> Tasks
    {
        get
        {
            lock (_sync)
                return _operations.Values.Select(operation => operation.Message)
                    .Where(message => !string.IsNullOrWhiteSpace(message)).Select(message => message!).ToArray();
        }
    }

    public GXProgressScope ProgressStart(string? message)
    {
        var scope = new GXProgressScope(ProgressEnd);
        lock (_sync) _operations.Add(scope.Id, (scope, message));
        Changed?.Invoke();
        return scope;
    }

    public void ProgressEnd(Guid id)
    {
        GXProgressScope scope;
        lock (_sync)
        {
            if (!_operations.Remove(id, out var operation)) return;
            scope = operation.Scope;
        }
        scope.Dispose();
        Changed?.Invoke();
    }

    public Task CancelAllAsync()
    {
        GXProgressScope[] scopes;
        lock (_sync) scopes = _operations.Values.Select(operation => operation.Scope).ToArray();
        return Task.WhenAll(scopes.Select(scope => scope.CancelAsync()));
    }
}
