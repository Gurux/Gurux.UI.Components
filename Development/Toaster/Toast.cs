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
        public string? Title { get; init; }

        /// <summary>
        /// Gets or sets the body of the toast message.
        /// </summary>
        public string? Message { get; init; }

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
        public DateTimeOffset? ClosingTime { get; init; }

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
