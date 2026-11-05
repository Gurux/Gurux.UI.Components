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

using System.Timers;

namespace Gurux.UI.Component.Toaster
{
    /// <summary>
    /// Stores transient toast messages, applies filters, and removes expired messages on a timer.
    /// </summary>
    public class GXToasterService : IGXToasterService, IDisposable
    {
        private readonly List<GXToast> _toastList = new List<GXToast>();
        private readonly List<string> _filter = [];
        private System.Timers.Timer _timer = new System.Timers.Timer();
        /// <summary>
        /// Occurs when a toast is added or removed.
        /// </summary>
        public event EventHandler? ToasterChanged;
        /// <summary>
        /// Occurs when the expiration timer removes one or more toast messages.
        /// </summary>
        public event EventHandler? ToasterTimerElapsed;

        /// <summary>
        /// Gets whether any toast messages are currently stored.
        /// </summary>
        public bool Any
        {
            get
            {
                return _toastList.Any();
            }
        }

        private int _maxCount = 20;

        /// <summary>Gets or sets the positive display limit, removing the oldest excess messages immediately.</summary>
        public int MaxCount
        {
            get => _maxCount;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _maxCount = value;
                if (_toastList.Count > value)
                {
                    _toastList.RemoveRange(0, _toastList.Count - value);
                    ToasterChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Creates a toaster service with a stored-message limit of twenty and a one-second expiration timer.
        /// </summary>
        public GXToasterService()
        {
            MaxCount = 20;
            _timer.Interval = 1000;
            _timer.AutoReset = true;
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        /// <summary>
        /// Removes expired messages and returns the stored toast list.
        /// </summary>
        public List<GXToast> GetToasts()
        {
            RemoveElapsed();
            return _toastList;
        }

        /// <summary>
        /// Removes expired toast messages and notifies listeners when expiration changes the displayed list.
        /// </summary>
        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (RemoveElapsed())
            {
                ToasterTimerElapsed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Adds a toast unless its title is filtered out, removes the oldest message at the count limit, and notifies listeners.
        /// </summary>
        public void Add(GXToast toast)
        {
            if (_filter.Contains(toast.Title ?? string.Empty))
            {
                return;
            }
            while (_toastList.Count >= MaxCount)
            {
                //Remove oldest item when toater is full.
                Remove(_toastList.First());
            }
            _toastList.Add(toast);
            if (!RemoveElapsed())
            {
                ToasterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Removes the specified toast and notifies subscribers that the toast list changed.
        /// </summary>
        public void Remove(GXToast toast)
        {
            if (_toastList.Contains(toast))
            {
                _toastList.Remove(toast);
                if (!RemoveElapsed())
                {
                    ToasterChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Removes toast messages whose closing time has passed and reports whether any were removed.
        /// </summary>
        private bool RemoveElapsed()
        {
            var removed = _toastList.Where(item => item.IsElapsed).ToList();
            if (removed != null && removed.Any())
            {
                removed.ForEach(toast => _toastList.Remove(toast));
                ToasterChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Detaches the expiration handler and stops the timer.
        /// </summary>
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= TimerElapsed;
                _timer.Stop();
            }
        }

        /// <summary>
        /// Replaces the title filter used to reject newly added toast messages.
        /// </summary>
        public void Filter(IEnumerable<string> filters)
        {
            _filter.Clear();
            _filter.AddRange(filters);
        }
    }
}
