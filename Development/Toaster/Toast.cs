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

namespace Gurux.UI.Component.Toaster
{
    /// <summary>
    /// Represents a transient message with a title, color, creation time, and optional expiration.
    /// </summary>
    public record GXToast
    {
        /// <summary>
        /// Stores the identifier associated with the toast.
        /// </summary>
        public Guid Id;

        /// <summary>
        /// Gets or sets the title displayed by the component.
        /// </summary>
        public string? Title
        {
            get; init;
        }

        /// <summary>
        /// Gets or sets the body of the toast message.
        /// </summary>
        public string? Message
        {
            get; init;
        }

        /// <summary>
        /// Gets or sets the Bootstrap contextual color of the toast.
        /// </summary>
        public Color Color { get; init; } = Color.Primary;

        /// <summary>
        /// Stores the local time at which the toast was created.
        /// </summary>
        public readonly DateTimeOffset CreationTime = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the time after which the toast is considered expired.
        /// </summary>
        public DateTimeOffset? ClosingTime
        {
            get; init;
        }

        /// <summary>
        /// Gets whether the toast has a closing time that has passed.
        /// </summary>
        public bool IsElapsed
        {
            get
            {
                return ClosingTime < DateTimeOffset.Now;
            }
        }

        /// <summary>
        /// Gets a human-readable description of the time elapsed since the toast was created.
        /// </summary>
        public string PostedTimeText
        {
            get
            {
                TimeSpan elapsedTime = CreationTime - DateTimeOffset.Now;
                return elapsedTime.Seconds > 60
                ? $"posted {-elapsedTime.Minutes} mins ago"
                : $"posted {-elapsedTime.Seconds} secs ago";
            }
        }

        /// <summary>
        /// Creates an empty toast with default styling and no expiration time.
        /// </summary>
        public GXToast()
        {

        }

        /// <summary>
        /// Creates a toast with its title, message, contextual color, and lifetime in seconds.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="messageColour">Color</param>
        /// <param name="secsToLive">Lifetime</param>
        public GXToast(string title, string? message, Color messageColour, int secsToLive)
        {
            Title = title;
            if (message != null)
            {
                Message = message.Replace(Environment.NewLine, "<br/>");
            }
            Color = messageColour;
            ClosingTime = DateTimeOffset.Now.AddSeconds(secsToLive);
        }
    }
}
