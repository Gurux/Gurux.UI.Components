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
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Gurux.UI.Components
{
    /// <summary>
    /// Renders a search input that publishes changes immediately or when Enter is pressed.
    /// </summary>
    public class Search : ComponentBase
    {
        /// <summary>
        /// Gets or sets the current search text.
        /// </summary>
        [Parameter]
        public string? Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the callback invoked when the bound value changes.
        /// </summary>
        [Parameter]
        public EventCallback<string?> ValueChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the most recently used value is restored from and saved to browser local storage.
        /// </summary>
        [Parameter]
        public bool LastValue { get; set; } = true;


        /// <summary>
        /// Gets or sets the injected browser local storage service used for value persistence.
        /// </summary>
        [Inject]
        IGXLocalStorage? localStorage { get; set; }

        /// <summary>
        /// Gets or sets the injected logger used to report component activity and errors.
        /// </summary>
        [Inject]
        ILogger<Search>? Logger { get; set; }

        /// <summary>
        /// Gets or sets whether filtering is triggered as the user types instead of waiting for Enter.
        /// </summary>
        [Parameter]
        public bool Immediate { get; set; }

        /// <summary>
        /// Gets or sets captured HTML attributes forwarded to the underlying element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        /// <summary>
        /// Gets the captured name attribute used by the search input.
        /// </summary>
        protected string NameAttributeValue
        {
            get
            {
                if (AdditionalAttributes?.TryGetValue("name", out var nameAttributeValue) ?? false)
                {
                    return Convert.ToString(nameAttributeValue, CultureInfo.InvariantCulture) ?? string.Empty;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the CSS classes obtained by combining component defaults with captured attributes.
        /// </summary>
        private string? CssClass => GXComponentAttributes.GetClass(AdditionalAttributes, "form-control");

        /// <summary>
        /// Gets the supplied HTML id or a Guid identifier generated once for this component instance.
        /// </summary>
        protected string Id => GXComponentAttributes.GetId(AdditionalAttributes, ref _generatedId);

        private string? _generatedId;

        /// <summary>
        /// Builds the component's HTML elements, attributes, and event handlers.
        /// </summary>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var seq = 0;
            builder.OpenElement(seq++, "input");
            builder.AddAttribute(seq++, "id", Id);
            builder.AddAttribute(seq++, "placeholder", Properties.Resources.Search);
            builder.AddAttribute(seq++, "type", "search");
            builder.AddMultipleAttributes(seq++, AdditionalAttributes);
            if (!string.IsNullOrEmpty(NameAttributeValue))
            {
                builder.AddAttribute(seq++, "name", NameAttributeValue);
            }
            if (!string.IsNullOrEmpty(CssClass))
            {
                builder.AddAttribute(seq++, "class", CssClass);
            }
            builder.AddAttribute(seq++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInputChanged));
            builder.AddAttribute(seq++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, OnKeydown));
            builder.AddAttribute(seq++, "value", Value);
            builder.CloseElement();
        }

        /// <summary>
        /// Updates the search text and publishes it when immediate filtering is enabled.
        /// </summary>
        private async Task OnInputChanged(ChangeEventArgs e)
        {
            Value = e.Value?.ToString();
            if (Immediate)
            {
                await UpdateValue();
            }
        }

        /// <summary>
        /// Publishes the search text when the Enter key is pressed.
        /// </summary>
        private async Task OnKeydown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await UpdateValue();
            }
        }

        /// <summary>
        /// Publishes the current value and saves it when persistence is enabled.
        /// </summary>
        private async Task UpdateValue()
        {
            try
            {
                //Update the new value.
                await ValueChanged.InvokeAsync(Value);
                if (localStorage != null && LastValue && !string.IsNullOrEmpty(Id))
                {
                    await localStorage.SetValueAsync(Id, Value);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Restores the most recently saved search text when persistence is enabled.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                //Get the default value from the cookies
                // if it's not set.
                if (localStorage != null && LastValue && !string.IsNullOrEmpty(Id))
                {
                    string? value = await localStorage.GetValueAsync(Id);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (Value != value)
                        {
                            Value = value;
                            if (ValueChanged.HasDelegate)
                            {
                                await ValueChanged.InvokeAsync(value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex.Message);
            }
        }
    }
}
