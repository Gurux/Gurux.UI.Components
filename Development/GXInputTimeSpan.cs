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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Gurux.UI.Components
{
    /// <summary>
    /// Edits a numeric duration using a time input and the configured duration unit.
    /// </summary>
    public class GXInputTimeSpan<TValue> : InputBase<TValue>
    {
        /// <summary>
        /// Gets the supplied HTML id or a Guid identifier generated once for this component instance.
        /// </summary>
        private string Id => GXComponentAttributes.GetId(AdditionalAttributes, ref _generatedId);

        private string? _generatedId;

        /// <summary>
        /// Gets or sets whether the most recently used value is restored from and saved to browser local storage.
        /// </summary>
        /// <seealso cref="Id" />
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
        ILogger<GXInputNumber<TValue>>? Logger { get; set; }

        /// <summary>
        /// Gets or sets whether the numeric duration represents seconds, minutes, or hours.
        /// </summary>
        [Parameter]
        public TimeSpanUnit TimeSpanUnit { get; set; } = TimeSpanUnit.Second;

        /// <summary>
        /// Gets or sets the time input element reference, available after rendering.
        /// </summary>
        [DisallowNull]
        public ElementReference? Element { get; protected set; }

        /// <summary>
        /// Creates the duration input and rejects unsupported bound value types.
        /// </summary>
        public GXInputTimeSpan()
        {
            Type type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            if (type != typeof(TimeSpan) && type != typeof(int) && type != typeof(UInt32) && type != typeof(TimeOnly))
            {
                throw new InvalidOperationException($"Unsupported {GetType()} type param '{type}'.");
            }
        }
        /// <summary>
        /// Restores a saved numeric duration when persistence is enabled.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (LastValue && !string.IsNullOrEmpty(Id) && localStorage != null)
                {
                    TValue? result;
                    string? value = await localStorage.GetValueAsync(Id);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.InvariantCulture, out result))
                        {
                            Value = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex.Message);
            }
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Gets the HTML time input increment in seconds corresponding to the configured duration unit.
        /// </summary>
        private int Step
        {
            get
            {
                int value = 60;
                switch (TimeSpanUnit)
                {
                    case TimeSpanUnit.Second:
                        value = 1;
                        break;
                    case TimeSpanUnit.Minute:
                        value = 60;
                        break;
                    case TimeSpanUnit.Hour:
                        value = 3600;
                        break;
                    default:
                        break;
                }
                return value;
            }
        }

        /// <summary>
        /// Builds the component's HTML elements, attributes, and event handlers.
        /// </summary>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "id", Id);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddAttribute(3, "type", "time");
            if (!string.IsNullOrEmpty(NameAttributeValue))
            {
                builder.AddAttribute(4, "name", NameAttributeValue);
            }
            builder.AddAttribute(5, "class", GXComponentAttributes.CombineClasses("form-control", CssClass));
            builder.AddAttribute(6, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(7, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.AddAttribute(8, "step", Step.ToString());
            builder.SetUpdatesAttributeName("value");
            builder.AddElementReferenceCapture(9, __inputReference => Element = __inputReference);
            builder.CloseElement();
        }

        /// <summary>
        /// Formats the numeric duration as a time value using the configured seconds, minutes, or hours unit.
        /// </summary>
        protected override string? FormatValueAsString(TValue? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            Type type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            if (value is int @int)
            {
                string result;
                switch (TimeSpanUnit)
                {
                    case TimeSpanUnit.Second:
                        result = TimeSpan.FromSeconds(@int).ToString();
                        break;
                    case TimeSpanUnit.Minute:
                        result = TimeSpan.FromMinutes(@int).ToString("hh\\:mm");
                        break;
                    case TimeSpanUnit.Hour:
                        result = TimeSpan.FromHours(@int).ToString();
                        break;
                    default:
                        result = "";
                        break;
                }
                return result;
            }
            if (value is UInt32 @uInt32)
            {
                string result;
                switch (TimeSpanUnit)
                {
                    case TimeSpanUnit.Second:
                        result = TimeSpan.FromSeconds(@uInt32).ToString();
                        break;
                    case TimeSpanUnit.Minute:
                        result = TimeSpan.FromMinutes(@uInt32).ToString("hh\\:mm");
                        break;
                    case TimeSpanUnit.Hour:
                        result = TimeSpan.FromHours(@uInt32).ToString();
                        break;
                    default:
                        result = "";
                        break;
                }
                return result;
            }
            return base.FormatValueAsString(value);
        }

        /// <summary>
        /// Parses time text as total seconds for integer values or converts it to the bound time type, then persists changes when enabled.
        /// </summary>
        protected override bool TryParseValueFromString(string? value,
            [MaybeNullWhen(false)] out TValue result,
            [NotNullWhen(false)] out string? validationErrorMessage)
        {
            validationErrorMessage = "";
            if (value != null)
            {
                Type type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
                if (type == typeof(int) || type == typeof(UInt32))
                {
                    try
                    {
                        int tmp = (int)TimeSpan.Parse(value).TotalSeconds;
                        if (!BindConverter.TryConvertTo(tmp.ToString(CultureInfo.InvariantCulture),
                            CultureInfo.InvariantCulture, out result))
                        {
                            validationErrorMessage = "Invalid time span.";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = default;
                        validationErrorMessage = ex.Message;
                        return false;
                    }
                }
                else if (!BindConverter.TryConvertTo<TValue>(value, CultureInfo.InvariantCulture, out result))
                {
                    validationErrorMessage = "Invalid time span.";
                    return false;
                }
                bool change = Value?.GetHashCode() != result?.GetHashCode();
                if (change)
                {
                    try
                    {
                        if (localStorage != null && LastValue && !string.IsNullOrEmpty(Id))
                        {
                            localStorage.SetValueAsync(Id, result?.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex.Message);
                        return false;
                    }
                }
            }
            else
            {
                result = default;
                return false;
            }
            return true;
        }
    }
}
