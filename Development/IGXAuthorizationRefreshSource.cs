namespace Gurux.UI.Components;

/// <summary>
/// Notifies authorization-aware components when policy decisions must be reevaluated.
/// </summary>
public interface IGXAuthorizationRefreshSource
{
    /// <summary>
    /// Occurs when authorization-aware components must reevaluate their policy decisions.
    /// </summary>
    event Action? Changed;
    /// <summary>
    /// Notifies subscribers that cached authorization decisions must be reevaluated.
    /// </summary>
    void Invalidate();
}
