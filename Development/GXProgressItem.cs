namespace Gurux.UI.Components;

/// <summary>
/// Describes the current progress of an active operation.
/// </summary>
public sealed record GXProgressItem(Guid Id, string? Message, int Completed = 0, int Total = 0);
