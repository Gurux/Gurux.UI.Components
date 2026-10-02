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
    /// Toast.
    /// </summary>
    public record GXToast
    {
        /// <summary>
        /// Toast ID.
        /// </summary>
        public Guid Id;

        /// <summary>
        /// Toast title.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Toast message.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Toast color.
        /// </summary>
        public Color Color { get; init; } = Color.Primary;

        /// <summary>
        /// Creation time.
        /// </summary>
        public readonly DateTimeOffset CreationTime = DateTimeOffset.Now;

        /// <summary>
        /// Closing time.
        /// </summary>
        public DateTimeOffset? ClosingTime { get; init; }

        /// <summary>
        /// Is time elapsed and toaster should remove.
        /// </summary>
        public bool IsElapsed
        {
            get
            {
                return ClosingTime < DateTimeOffset.Now;
            }
        }

        /// <summary>
        /// Get posted time text.
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
        /// Constructor.
        /// </summary>
        public GXToast()
        {

        }

        /// <summary>
        /// Constructor.
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