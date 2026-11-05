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

namespace Gurux.UI.Components
{
    /// <summary>
    /// Describes a notification's severity, content, grouping key, dismissibility, and display scope.
    /// </summary>
    /// <param name="Level">The notification severity.</param>
    /// <param name="Title">The notification title.</param>
    /// <param name="Description">The notification details.</param>
    /// <param name="Key">An optional key used to remove related notifications.</param>
    /// <param name="Closable">Whether the notification can be dismissed.</param>
    /// <param name="Scope">Whether the notification applies to a page or the application.</param>
    public sealed record GXNotificationItem
    (
        NotificationLevel Level,
        string? Title,
        string Description,
        string? Key = null,
        bool Closable = false,
        NotificationScope Scope = NotificationScope.Page
    )
    {
        /// <summary>
        /// Gets the Guid used to identify and remove this notification.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();
        /// <summary>Optional trusted UI content for the title. Plain text remains the default.</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Microsoft.AspNetCore.Components.RenderFragment? TitleContent { get; init; }
        /// <summary>
        /// Gets the notification creation time used to order notifications.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Stores user notifications and exposes display settings and change notifications.
    /// </summary>
    public interface IGXNotificationService
    {
        /// <summary>
        /// Gets or sets the maximum number of displayed notifications; nonpositive values show all matching notifications.
        /// </summary>
        public int MaxVisibleNotifications { get; set; }

        /// <summary>
        /// Gets or sets the optional notification fields displayed by consumers.
        /// </summary>
        NotificationFields VisibleFields { get; set; }

        /// <summary>
        /// Gets or sets the notification severity flags included in the displayed list.
        /// </summary>
        NotificationLevel LevelFilter { get; set; }

        /// <summary>
        /// Stores the supplied notification and returns its identifier.
        /// </summary>
        /// <returns>The identifier of the notification.</returns>
        Guid Add(GXNotificationItem item);

        /// <summary>
        /// Creates an error notification using the exception message and details.
        /// </summary>
        /// <param name="exception">The exception to report.</param>
        /// <param name="key">An optional grouping key used to replace or remove related notifications.</param>
        /// <param name="scope">Whether the error applies to the current page or the entire application.</param>
        /// <returns>The identifier of the error notification.</returns>
        Guid ReportError(Exception exception,
            string? key = null,
            NotificationScope scope = NotificationScope.Page);

        /// <summary>
        /// Removes the notification with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the notification to remove.</param>
        void Remove(Guid id);

        /// <summary>
        /// Removes every notification with the specified grouping key.
        /// </summary>
        void RemoveByKey(string key);

        /// <summary>
        /// Removes all stored notifications.
        /// </summary>
        void Clear();

        /// <summary>
        /// Occurs when notifications or their display settings change.
        /// </summary>
        event Action? Changed;

        /// <summary>
        /// Gets a snapshot of all stored notifications regardless of severity filters or display limits.
        /// </summary>
        IReadOnlyList<GXNotificationItem> Notifications
        {
            get;
        }
    }
}

