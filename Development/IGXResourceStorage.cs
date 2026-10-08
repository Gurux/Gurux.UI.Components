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

using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Gurux.UI.Components
{
    /// <summary>
    /// Carries the previous and replacement text for a localization change.
    /// </summary>
    public class GXTextChangedEventArgs
    {
        /// <summary>
        /// Gets or sets the resource text before the localization change.
        /// </summary>
        public string? PreviousText { get; set; }

        /// <summary>
        /// Gets or sets the replacement localized text.
        /// </summary>
        public string? CurrentText { get; set; }
    }

    /// <summary>
    /// Provides localized text, images, themes, enumeration labels, and user presentation preferences.
    /// </summary>
    public interface IGXResourceStorage
    {
        /// <summary>
        /// Retrieves the user's preferred language from the configured resource storage.
        /// </summary>
        Task<string?> GetUserLanguageAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores the user's preferred language.
        /// </summary>
        /// <param name="value">The user language string to set.</param>
        /// <param name="cancellationToken">The token used to cancel the language update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SetUserLanguageAsync(string? value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Occurs when the user's selected language changes.
        /// </summary>
        public event EventHandler<string?> OnLanguageChanged;

        /// <summary>
        /// Refreshes the localization settings used to retrieve translated resources.
        /// </summary>
        Task UpdateLocalizationAsync();

        /// <summary>
        /// Retrieves the user's current theme settings.
        /// </summary>
        Task<GXThemeInfo?> GetCurrentThemeAsync();

        /// <summary>
        /// Retrieves the available icon packs and their resource locations.
        /// </summary>
        Task<IEnumerable<string>?> GetCurrentIconPacksAsync();

        /// <summary>
        /// Retrieves the image resource with the specified name.
        /// </summary>
        Task<string?> GetImageAsync(string name);

        /// <summary>
        /// Retrieves the image resources with the specified names.
        /// </summary>
        /// <param name="names">Image names.</param>
        /// <returns>List of images.</returns>
        Task<List<string?>?> GetImagesAsync(IEnumerable<string> names);

        /// <summary>
        /// Reloads enumeration labels of the specified type from the server.
        /// </summary>
        /// <param name="type">Enum type name.</param>
        /// <returns>Return an array of { id, name } items for the enum.</returns>
        Task<IEnumerable<KeyValuePair<int, string>>?> RefreshEnumTypesAsync(string type);

        /// <summary>
        /// Retrieves the display label for an enumeration value of the specified type.
        /// </summary>
        /// <param name="type">Enum type name.</param>
        /// <param name="id">Enum ID.</param>
        /// <returns>Enum type name.</returns>
        Task<string?> GetEnumTypeAsync(string type, int id);

        /// <summary>
        /// Retrieves display labels for the specified enumeration values.
        /// </summary>
        /// <param name="type">Enum type name.</param>
        /// <param name="ids">Enum IDs.</param>
        /// <returns>List of enum types.</returns>
        Task<List<string?>?> GetEnumTypesAsync(string type, IEnumerable<int> ids);

        /// <summary>
        /// Returns the cached display label for an enumeration value.
        /// </summary>
        /// <param name="type">Enum type name.</param>
        /// <param name="id">Enum ID.</param>
        /// <returns>Enum type name.</returns>
        string? GetEnumType(string type, int? id);

        /// <summary>
        /// Returns the cached display label for an enumeration value.
        /// </summary>
        /// <param name="type">Enum type name.</param>
        /// <param name="value">Enumerated value.</param>
        /// <returns>Enum type name.</returns>
        string? GetEnumType(string type, Enum value);

        /// <summary>
        /// Updates the cached identifier-to-label mappings for an enumeration type.
        /// </summary>
        /// <param name="type">Enum type name.</param>
        /// <param name="values">key value pairs.</param>
        /// <returns>Return an array of { id, name } items for the enum.</returns>
        void UpdateEnumTypes(string type, IEnumerable<KeyValuePair<int, string>> values);

        /// <summary>
        /// Clears the cached labels for the specified enumeration type.
        /// </summary>
        /// <param name="type">Enum type name.</param>
        void ClearEnumTypes(string type);

        /// <summary>
        /// Retrieves a translated resource string using the specified or current culture.
        /// </summary>
        /// <param name="cultureInfo">Used culture.</param>
        /// <param name="name">Localized name.</param>
        /// <returns>localized text</returns>
        Task<string> GetLocalizedTextAsync(CultureInfo cultureInfo, string name);

        /// <summary>
        /// Retrieves a translated resource string using the specified or current culture.
        /// </summary>
        /// <param name="name">Localized name.</param>
        /// <returns>localized text</returns>
        Task<string> GetLocalizedTextAsync(string name);

        /// <summary>
        /// Retrieves translated resource strings using the specified or current culture.
        /// </summary>
        /// <param name="cultureInfo">Used culture.</param>
        /// <param name="names">Localized names.</param>
        /// <returns>List of localized texts.</returns>
        Task<List<string>> GetLocalizedTextsAsync(CultureInfo cultureInfo, IEnumerable<string> names);

        /// <summary>
        /// Retrieves translated resource strings using the specified or current culture.
        /// </summary>
        /// <param name="names">Localized names.</param>
        /// <returns>List of localized texts.</returns>
        Task<List<string>> GetLocalizedTextsAsync(IEnumerable<string> names);

        /// <summary>
        /// Occurs when the current theme settings change.
        /// </summary>
        public event EventHandler<GXThemeInfo> OnThemeChanged;

        /// <summary>
        /// Occurs when cached enumeration labels change.
        /// </summary>
        public EventCallback OnEnumTypesChanged { get; set; }

        /// <summary>
        /// Occurs when a localized text resource is replaced.
        /// </summary>
        public event EventHandler<GXTextChangedEventArgs>? OnTextChanged;

        /// <summary>
        /// Occurs when an image resource changes.
        /// </summary>
        public event EventHandler<GXImageChangedArgs>? OnImageChanged;

        /// <summary>
        /// Formats a date and time value using the user's configured time display preference.
        /// </summary>
        /// <remarks>The format of the returned string depends on the default formatting conventions for <see cref="DateTimeOffset" />.</remarks>
        /// <param name="value">The <see cref="DateTimeOffset" /> value to convert.</param>
        /// <returns>A string representation of the specified <see cref="DateTimeOffset" /> value.</returns>
        /// <seealso cref="HasLocalDateTimeAsync" />
        public string? DateTimeOffsetToString(DateTimeOffset? value);

        /// <summary>
        /// Retrieves whether date and time values should be displayed in local time instead of UTC.
        /// </summary>
        /// <returns>
        ///   <see langword="true" /> if the current instance contains a local time; otherwise, UTC time is used.</returns>
        /// <seealso cref="DateTimeOffsetToString" />
        /// <seealso cref="SetLocalDateTimeAsync" />
        public Task<bool> HasLocalDateTimeAsync();

        /// <summary>
        /// Stores whether date and time values should be displayed in local time instead of UTC.
        /// </summary>
        /// <param name="value">A boolean value indicating whether the local ot UTC time is used.</param>
        /// <seealso cref="DateTimeOffsetToString" />
        /// <seealso cref="HasLocalDateTimeAsync" />
        public Task SetLocalDateTimeAsync(bool value);
    }
}
