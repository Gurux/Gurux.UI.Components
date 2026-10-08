namespace Gurux.UI.Components.Enums
{
    /// <summary>
    /// Specifies notification severity flags used for styling and filtering.
    /// </summary>
    [Flags]
    public enum NotificationLevel
    {
        /// <summary>
        /// Includes no notification severities.
        /// </summary>
        None = 0,
        /// <summary>
        /// Represents an informational notification.
        /// </summary>
        Information = 1,
        /// <summary>
        /// Represents a successful operation.
        /// </summary>
        Success = 2,
        /// <summary>
        /// Represents a warning that may need attention.
        /// </summary>
        Warning = 4,
        /// <summary>
        /// Represents a failed operation or error condition.
        /// </summary>
        Error = 8
    }
}