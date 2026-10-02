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
    public sealed record GXNotificationItem
    (
        NotificationLevel Level,
        string Title,
        string Description,
        string? Key = null,
        bool Closable = false,
        NotificationScope Scope = NotificationScope.Page
    )
    {
        /// <summary>
        /// Notification identifier. This is used to remove the notification from the list.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();
        /// <summary>
        /// Notification creation time. This is used to sort the notifications in the list.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Provides methods to display and manage user notifications.
    /// </summary>
    public interface IGXNotificationService
    {
        /// <summary>
        /// Maximum number of notifications shown. Zero or a negative value shows all notifications.
        /// </summary>
        public int MaxVisibleNotifications { get; set; }

        /// <summary>Optional fields to display. Defaults to Title.</summary>
        NotificationFields VisibleFields { get; set; }

        /// <summary>Levels to display. Defaults to all levels; None hides all notifications.</summary>
        NotificationLevel LevelFilter { get; set; }

        /// <summary>
        /// Displays a notification with the specified severity, title, and description.
        /// </summary>
        /// <param name="level">The severity of the notification.</param>
        /// <param name="title">The notification title.</param>
        /// <param name="description">The notification description.</param>
        /// <param name="closable">Whether the user can close the notification.</param>
        /// <returns>The identifier of the notification.</returns>
        Guid Add(GXNotificationItem item);

        /// <summary>
        /// Displays an error notification for the specified exception.
        /// </summary>
        /// <param name="exception">The exception to report.</param>
        /// <returns>The identifier of the error notification.</returns>
        Guid ReportError(Exception exception,
            string? key = null,
            NotificationScope scope = NotificationScope.Page);

        /// <summary>
        /// Removes the notification with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the notification to remove.</param>
        void Remove(Guid id);

        void RemoveByKey(string key);

        /// <summary>
        /// Removes all notifications.
        /// </summary>
        void Clear();

        event Action? Changed;

        IReadOnlyList<GXNotificationItem> Notifications { get; }
    }
}

