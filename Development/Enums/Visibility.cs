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

namespace Gurux.UI.Components.Enums
{
    /// <summary>
    /// Defines responsive breakpoint and print visibility options for table cells.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Displays the cell at every screen size.
        /// </summary>
        All,
        /// <summary>
        /// Displays the cell only at the largest configured responsive breakpoint.
        /// </summary>
        ExtraLargeLarge,
        /// <summary>
        /// Displays the cell at extra-large screen sizes and above.
        /// </summary>
        ExtraLarge,
        /// <summary>
        /// Displays the cell at large screen sizes and above.
        /// </summary>
        Large,
        /// <summary>
        /// Displays the cell at medium screen sizes and above.
        /// </summary>
        Medium,
        /// <summary>
        /// Displays the cell at small screen sizes and above.
        /// </summary>
        Small,
        /// <summary>
        /// Displays the cell at extra-small screen sizes and above.
        /// </summary>
        ExtraSmall,
        /// <summary>
        /// Shows the cell in printed output.
        /// </summary>
        Print,
        /// <summary>
        /// Hides the cell in printed output.
        /// </summary>
        PrintHide
    }
}
