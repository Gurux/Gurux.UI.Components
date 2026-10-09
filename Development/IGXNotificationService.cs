//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------
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

