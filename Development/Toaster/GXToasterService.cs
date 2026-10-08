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

        /// <summary>
        /// Gets or sets the toast count at which adding a message removes the oldest stored message.
        /// </summary>
        public int MaxCount
        {
            get;
            set;
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
            if (MaxCount == _toastList.Count)
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
