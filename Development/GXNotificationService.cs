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

namespace Gurux.UI.Components;

/// <summary>Stores notifications for the lifetime of this service instance.</summary>
public sealed class GXNotificationService : IGXNotificationService
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, GXNotificationItem> _notifications = new();
    private int _maxVisibleNotifications;
    private NotificationFields _visibleFields = NotificationFields.Title;
    private NotificationLevel _levelFilter = NotificationLevel.Information | NotificationLevel.Success |
        NotificationLevel.Warning | NotificationLevel.Error;

    public NotificationFields VisibleFields
    {
        get { lock (_sync) return _visibleFields; }
        set
        {
            lock (_sync)
            {
                if (_visibleFields == value) return;
                _visibleFields = value;
            }
            Changed?.Invoke();
        }
    }

    public NotificationLevel LevelFilter
    {
        get { lock (_sync) return _levelFilter; }
        set
        {
            lock (_sync)
            {
                if (_levelFilter == value) return;
                _levelFilter = value;
            }
            Changed?.Invoke();
        }
    }

    public int MaxVisibleNotifications
    {
        get { lock (_sync) return _maxVisibleNotifications; }
        set
        {
            lock (_sync)
            {
                if (_maxVisibleNotifications == value) return;
                _maxVisibleNotifications = value;
            }
            Changed?.Invoke();
        }
    }

    /// <summary>Returns a snapshot of all stored notifications, regardless of the display limit.</summary>
    public IReadOnlyList<GXNotificationItem> Notifications
    {
        get { lock (_sync) return _notifications.Values.ToArray(); }
    }

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
                    notifications = notifications.Take(_maxVisibleNotifications);
                return notifications.ToArray();
            }
        }
    }

    public event Action? Changed;

    public void Clear()
    {
        lock (_sync)
        {
            if (_notifications.Count == 0) return;
            _notifications.Clear();
        }
        Changed?.Invoke();
    }

    public Guid Add(GXNotificationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_sync) _notifications[item.Id] = item;
        Changed?.Invoke();
        return item.Id;
    }

    public void Remove(Guid id)
    {
        lock (_sync)
        {
            if (!_notifications.Remove(id)) return;
        }
        Changed?.Invoke();
    }

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
            }
        }
        Changed?.Invoke();
    }

    public Guid ReportError(Exception exception, string? key = null, NotificationScope scope = NotificationScope.Page)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return Add(new GXNotificationItem(NotificationLevel.Error,
            exception.Message, exception.ToString(), key, true, scope));
    }
}
