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
using Gurux.UI.Components.Tab;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Gurux.UI.Components;

/// <summary>
/// Stores notifications and exposes filtered snapshots for the lifetime of the service instance.
/// </summary>
public sealed class GXNotificationService : IGXNotificationService, IDisposable
{
    private readonly NavigationManager? _navigation;
    private readonly ITabRouteResolver? _tabRoutes;
    private readonly Dictionary<Guid, string> _notificationPages = new();

    /// <summary>Tracks successful navigation when a navigation manager is available through dependency injection.</summary>
    public GXNotificationService(NavigationManager? navigation = null, ITabRouteResolver? tabRoutes = null)
    {
        _navigation = navigation;
        _tabRoutes = tabRoutes;
        if (_navigation != null)
        {
            _navigation.LocationChanged += LocationChanged;
        }
    }

    private void LocationChanged(object? sender, LocationChangedEventArgs args)
    {
        bool changed = false;
        lock (_sync)
        {
            var ids = _notifications.Values.Where(item => item.Scope != NotificationScope.Global &&
                (!_notificationPages.TryGetValue(item.Id, out var page) ||
                 !(_tabRoutes?.IsTabNavigation(page, args.Location) ?? TabUrlState.IsTabNavigation(page, args.Location))))
                .Select(item => item.Id).ToArray();
            foreach (var id in ids) { _notifications.Remove(id); _notificationPages.Remove(id); changed = true; }
        }
        if (changed)
        {
            Changed?.Invoke();
        }
    }

    /// <summary>Releases the navigation subscription owned by this service scope.</summary>
    public void Dispose()
    {
        if (_navigation != null)
        {
            _navigation.LocationChanged -= LocationChanged;
        }
    }
    private readonly object _sync = new();
    private readonly Dictionary<Guid, GXNotificationItem> _notifications = new();
    private int _maxVisibleNotifications;
    private NotificationFields _visibleFields = NotificationFields.Title;
    private NotificationLevel _levelFilter = NotificationLevel.Information | NotificationLevel.Success |
        NotificationLevel.Warning | NotificationLevel.Error;

    /// <summary>
    /// Gets or sets the optional notification fields displayed by consumers.
    /// </summary>
    public NotificationFields VisibleFields
    {
        get
        {
            lock (_sync)
            {
                return _visibleFields;
            }
        }
        set
        {
            lock (_sync)
            {
                if (_visibleFields == value)
                {
                    return;
                }
                _visibleFields = value;
            }
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets the notification severity flags included in the displayed list.
    /// </summary>
    public NotificationLevel LevelFilter
    {
        get
        {
            lock (_sync)
            {
                return _levelFilter;
            }
        }
        set
        {
            lock (_sync)
            {
                if (_levelFilter == value)
                {
                    return;
                }
                _levelFilter = value;
            }
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of displayed notifications; nonpositive values show all matching notifications.
    /// </summary>
    public int MaxVisibleNotifications
    {
        get
        {
            lock (_sync)
            {
                return _maxVisibleNotifications;
            }
        }
        set
        {
            lock (_sync)
            {
                if (_maxVisibleNotifications == value)
                {
                    return;
                }
                _maxVisibleNotifications = value;
            }
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Gets a snapshot of all stored notifications regardless of severity filters or display limits.
    /// </summary>
    public IReadOnlyList<GXNotificationItem> Notifications
    {
        get
        {
            lock (_sync)
            {
                return _notifications.Values.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets matching notifications ordered newest first and limited by the configured display count.
    /// </summary>
    public IReadOnlyList<GXNotificationItem> VisibleNotifications
    {
        get
        {
            lock (_sync)
            {
                IEnumerable<GXNotificationItem> notifications = _notifications.Values
                    .Where(item => (item.Level & _levelFilter) != NotificationLevel.None)
                    .OrderByDescending(item => item.CreatedAt);
                if (_maxVisibleNotifications > 0)
                {
                    notifications = notifications.Take(_maxVisibleNotifications);
                }
                return notifications.ToArray();
            }
        }
    }

    /// <summary>
    /// Occurs when stored notifications or notification display settings change.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Removes all stored notifications and notifies listeners when the list was nonempty.
    /// </summary>
    public void Clear()
    {
        lock (_sync)
        {
            if (_notifications.Count == 0)
            {
                return;
            }
            _notifications.Clear();
            _notificationPages.Clear();
        }
        Changed?.Invoke();
    }

    /// <summary>
    /// Stores the notification by its id, replaces any existing entry with that id, and notifies listeners.
    /// </summary>
    public Guid Add(GXNotificationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_sync)
        {
            _notifications[item.Id] = item;
            if (_navigation != null && item.Scope != NotificationScope.Global)
            {
                _notificationPages[item.Id] = _navigation.Uri;
            }
            else
            {
                _notificationPages.Remove(item.Id);
            }
        }

        Changed?.Invoke();
        return item.Id;
    }

    /// <summary>
    /// Removes the notification with the specified id and notifies listeners when an entry was removed.
    /// </summary>
    public void Remove(Guid id)
    {
        lock (_sync)
        {
            _notificationPages.Remove(id);
            if (!_notifications.Remove(id))
            {
                return;
            }
        }
        Changed?.Invoke();
    }

    /// <summary>
    /// Removes every notification with the specified grouping key.
    /// </summary>
    public void RemoveByKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        lock (_sync)
        {
            Guid[] ids = _notifications.Values.Where(item => item.Key == key)
                .Select(item => item.Id).ToArray();
            foreach (Guid id in ids)
            {
                _notifications.Remove(id);
                _notificationPages.Remove(id);
            }
        }
        Changed?.Invoke();
    }

    /// <summary>
    /// Creates an error notification using the exception message and details.
    /// </summary>
    public Guid ReportError(Exception exception, string? key = null, NotificationScope scope = NotificationScope.Page)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return Add(new GXNotificationItem(NotificationLevel.Error,
            exception.Message, exception.ToString(), key, true, scope));
    }
}
