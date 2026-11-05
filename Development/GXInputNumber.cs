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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Gurux.UI.Components
{
    /// <summary>
    /// Renders a numeric form input with type-aware parsing, bounds, and optional value persistence.
    /// </summary>
    public class GXInputNumber<TValue> : InputBase<TValue>
    {
        private TValue? _min;
        private TValue? _max;

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
        public bool RememberValue { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum value accepted by the numeric input.
        /// </summary>
        [Parameter]
        public TValue? Min
        {
            get
            {
                return _min;
            }
            set
            {
                _min = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value accepted by the numeric input.
        /// </summary>
        [Parameter]
        public TValue? Max
        {
            get
            {
                return _max;
            }
            set
            {
                _max = value;
            }
        }

        /// <summary>
        /// Gets or sets the numeric increment; zero renders the HTML step value any.
        /// </summary>
        [Parameter]
        public int Step { get; set; } = 1;

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
        /// Gets or sets the validation message format used when the input cannot be parsed.
        /// </summary>
        [Parameter]
        public string ParsingErrorMessage { get; set; } = "The {0} field must be a number.";

        /// <summary>
        /// Gets or sets the numeric input element reference, available after rendering.
        /// </summary>
        [DisallowNull]
        public ElementReference? Element { get; protected set; }

        /// <summary>
        /// Initializes supported numeric bounds and restores the saved value when persistence is enabled.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            //Initialize min and max values if not set.
            if (typeof(TValue) == typeof(byte))
            {
                if (Min == null)
                {
                    BindConverter.TryConvertTo<TValue>(byte.MinValue, CultureInfo.InvariantCulture, out _min);
                }
                if (Max == null)
                {
                    BindConverter.TryConvertTo<TValue>(byte.MaxValue, CultureInfo.InvariantCulture, out _max);
                }
            }
            else if (typeof(TValue) == typeof(UInt16))
            {
                if (Min == null)
                {
                    BindConverter.TryConvertTo<TValue>(UInt16.MinValue, CultureInfo.InvariantCulture, out _min);
                }
                if (Max == null)
                {
                    BindConverter.TryConvertTo<TValue>(UInt16.MaxValue, CultureInfo.InvariantCulture, out _max);
                }
            }
            else if (typeof(TValue) == typeof(UInt32))
            {
                if (Min == null)
                {
                    BindConverter.TryConvertTo<TValue>(UInt32.MinValue, CultureInfo.InvariantCulture, out _min);
                }
                if (Max == null)
                {
                    BindConverter.TryConvertTo<TValue>(UInt32.MaxValue, CultureInfo.InvariantCulture, out _max);
                }
            }
            else if (typeof(TValue) == typeof(UInt64))
            {
                if (Min == null)
                {
                    BindConverter.TryConvertTo<TValue>(UInt64.MinValue, CultureInfo.InvariantCulture, out _min);
                }
                if (Max == null)
                {
                    BindConverter.TryConvertTo<TValue>(UInt64.MaxValue, CultureInfo.InvariantCulture, out _max);
                }
            }
            try
            {
                if (RememberValue && !string.IsNullOrEmpty(Id) && localStorage != null)
                {
                    TValue? result;
                    string? value = await localStorage.GetValueAsync(Id);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.InvariantCulture, out result))
                        {
                            CurrentValue = result;
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
        /// Builds the component's HTML elements, attributes, and event handlers.
        /// </summary>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "id", Id);
            builder.AddAttribute(2, "step", Step == 0 ? "any" : Step);
            builder.AddMultipleAttributes(3, AdditionalAttributes);
            builder.AddAttribute(4, "type", "number");
            if (!string.IsNullOrEmpty(NameAttributeValue))
            {
                builder.AddAttribute(5, "name", NameAttributeValue);
            }
            builder.AddAttribute(6, "class", GXComponentAttributes.CombineClasses("form-control", CssClass));
            builder.AddAttribute(7, "value", CurrentValueAsString);
            builder.AddAttribute(8, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.SetUpdatesAttributeName("value");
            if (Min != null)
            {
                builder.AddAttribute(9, "min", Min);
            }
            if (Max != null)
            {
                builder.AddAttribute(10, "max", Max);
            }
            builder.AddElementReferenceCapture(11, __inputReference => Element = __inputReference);
            builder.CloseElement();
        }

        /// <summary>
        /// Converts the input text to the bound type and supplies a validation message when conversion fails.
        /// </summary>
        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.InvariantCulture, out result))
            {
                validationErrorMessage = null;
                bool change = Value?.GetHashCode() != result?.GetHashCode();
                if (change)
                {
                    try
                    {
                        if (localStorage != null && RememberValue && !string.IsNullOrEmpty(Id))
                        {
                            localStorage.SetValueAsync(Id, result?.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex.Message);
                    }
                }
                return true;
            }
            else
            {
                validationErrorMessage = string.Format(CultureInfo.InvariantCulture, ParsingErrorMessage, DisplayName ?? FieldIdentifier.FieldName);
                return false;
            }
        }

        /// <summary>
        /// Formats the supported numeric value using invariant culture for the HTML number input.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string representation of the value.</returns>
        protected override string? FormatValueAsString(TValue? value)
        {
            // Avoiding a cast to IFormattable to avoid boxing.
            switch (value)
            {
                case null:
                    return null;

                case int @int:
                    return BindConverter.FormatValue(@int, CultureInfo.InvariantCulture);

                case long @long:
                    return BindConverter.FormatValue(@long, CultureInfo.InvariantCulture);

                case short @short:
                    return BindConverter.FormatValue(@short, CultureInfo.InvariantCulture);

                case float @float:
                    return BindConverter.FormatValue(@float, CultureInfo.InvariantCulture);

                case double @double:
                    return BindConverter.FormatValue(@double, CultureInfo.InvariantCulture);

                case decimal @decimal:
                    return BindConverter.FormatValue(@decimal, CultureInfo.InvariantCulture);
                case byte @byte:
                    return Convert.ToString(BindConverter.FormatValue(@byte, CultureInfo.InvariantCulture));
                case UInt16 @uInt16:
                    return Convert.ToString(BindConverter.FormatValue(@uInt16, CultureInfo.InvariantCulture));
                case UInt32 @uInt32:
                    return Convert.ToString(BindConverter.FormatValue(@uInt32, CultureInfo.InvariantCulture));
                case UInt64 @uInt64:
                    return Convert.ToString(BindConverter.FormatValue(@uInt64, CultureInfo.InvariantCulture));
                default:
                    throw new InvalidOperationException($"Unsupported type {value.GetType()}");
            }
        }
    }
}
