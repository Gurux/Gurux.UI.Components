namespace Gurux.UI.Components.Enums;

/// <summary>
/// Specifies optional notification fields to display; flags can be combined.
/// </summary>
[Flags]
public enum NotificationFields
{
    /// <summary>
    /// Displays no optional notification fields.
    /// </summary>
    None = 0,
    /// <summary>
    /// Displays the notification title.
    /// </summary>
    Title = 1,
    /// <summary>
    /// Displays the notification creation time.
    /// </summary>
    CreatedAt = 2,
    /// <summary>
    /// Displays the notification identifier.
    /// </summary>
    Id = 4,
    /// <summary>
    /// Displays every optional notification field.
    /// </summary>
    All = Title | CreatedAt | Id
}