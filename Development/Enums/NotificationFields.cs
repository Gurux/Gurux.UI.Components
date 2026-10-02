namespace Gurux.UI.Components.Enums;

/// <summary>Optional fields displayed in a notification.</summary>
[Flags]
public enum NotificationFields
{
    None = 0,
    Title = 1,
    CreatedAt = 2,
    Id = 4,
    All = Title | CreatedAt | Id
}