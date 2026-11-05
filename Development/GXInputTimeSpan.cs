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
        public bool RememberValue { get; set; } = true;

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
        /// Gets or sets whether the duration's whole days are edited separately from its clock component.
        /// </summary>
        [Parameter]
        public bool? Days { get; set; }

        private string? _invalidDays;

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Days == true)
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "d-inline-block");
                builder.OpenElement(2, "input");
                builder.AddAttribute(3, "id", Id + "-days");
                builder.AddAttribute(4, "type", "number");
                builder.AddAttribute(5, "step", "1");
                builder.AddAttribute(6, "min", "0");
                builder.AddAttribute(7, "max", "365");
                builder.AddAttribute(8, "class", GXComponentAttributes.CombineClasses("form-control", CssClass));
                builder.AddAttribute(9, "style", GXComponentAttributes.GetStyle(AdditionalAttributes));
                builder.AddAttribute(10, "disabled", GXComponentAttributes.GetValue(AdditionalAttributes, "disabled"));
                builder.AddAttribute(11, "value", _invalidDays ?? (CurrentValue is null ? null : GetDuration(CurrentValue).Days.ToString(CultureInfo.InvariantCulture)));
                builder.AddAttribute(12, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnDaysChanged));
                builder.SetUpdatesAttributeName("value");
                builder.CloseElement();
            }
            builder.OpenElement(20, "input");
            builder.AddAttribute(21, "id", Id);
            builder.AddMultipleAttributes(22, AdditionalAttributes);
            builder.AddAttribute(23, "type", "time");
            if (!string.IsNullOrEmpty(NameAttributeValue))
            {
                builder.AddAttribute(24, "name", NameAttributeValue);
            }
            builder.AddAttribute(25, "class", GXComponentAttributes.CombineClasses("form-control", CssClass));
            builder.AddAttribute(26, "value", _invalidDays is null ? CurrentValueAsString : FormatValueAsString(CurrentValue));
            builder.AddAttribute(27, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnTimeChanged));
            builder.AddAttribute(28, "step", Step.ToString(CultureInfo.InvariantCulture));
            builder.SetUpdatesAttributeName("value");
            builder.AddElementReferenceCapture(29, reference => Element = reference);
            builder.CloseElement();
            if (Days == true)
            {
                builder.CloseElement();
            }
        }

        private TimeSpan GetDuration(TValue? value)
        {
            if (value is null)
            {
                return TimeSpan.Zero;
            }
            if (value is TimeSpan duration)
            {
                return duration;
            }
            if (value is TimeOnly clock)
            {
                return clock.ToTimeSpan();
            }
            long integer = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            return TimeSpan.FromTicks(checked(integer * UnitTicks));
        }

        private long UnitTicks => TimeSpanUnit switch
        {
            TimeSpanUnit.Second => TimeSpan.TicksPerSecond,
            TimeSpanUnit.Minute => TimeSpan.TicksPerMinute,
            TimeSpanUnit.Hour => TimeSpan.TicksPerHour,
            _ => throw new ArgumentOutOfRangeException(nameof(TimeSpanUnit))
        };

        private async Task ApplyTextAsync(string? text)
        {
            TValue? previous = CurrentValue;
            CurrentValueAsString = text;
            if (!EqualityComparer<TValue>.Default.Equals(previous, CurrentValue) && RememberValue && localStorage != null)
            {
                try
                {
                    await localStorage.SetValueAsync(Id, CurrentValue is IFormattable formatted
                        ? formatted.ToString(null, CultureInfo.InvariantCulture) : CurrentValue?.ToString());
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex.Message);
                }
            }
        }

        private Task OnTimeChanged(ChangeEventArgs args)
        {
            _invalidDays = null;
            string? text = Convert.ToString(args.Value, CultureInfo.InvariantCulture);
            if (Days == true && TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var clock))
            {
                try
                {
                    text = (TimeSpan.FromDays(GetDuration(CurrentValue).Days) + clock).ToString("c", CultureInfo.InvariantCulture);
                }
                catch (OverflowException)
                {
                    text = "Invalid time span.";
                }
            }
            return ApplyTextAsync(text);
        }

        private Task OnDaysChanged(ChangeEventArgs args)
        {
            string? text = Convert.ToString(args.Value, CultureInfo.InvariantCulture);
            _invalidDays = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return ApplyTextAsync(null);
            }
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int days) && days >= 0 && days <= 365)
            {
                var current = GetDuration(CurrentValue);
                return ApplyTextAsync((TimeSpan.FromDays(days) + current - TimeSpan.FromDays(current.Days)).ToString("c", CultureInfo.InvariantCulture));
            }
            _invalidDays = text;
            return ApplyTextAsync("Invalid day value.");
        }

        /// <inheritdoc />
        protected override string? FormatValueAsString(TValue? value)
        {
            if (value is null)
            {
                return null;
            }
            TimeSpan duration = GetDuration(value);
            // Native time inputs accept clock text only; whole days have a separate editor.
            return duration.ToString(TimeSpanUnit == TimeSpanUnit.Minute ? @"hh\:mm" : @"hh\:mm\:ss", CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        protected override bool TryParseValueFromString(string? value,
            [MaybeNullWhen(false)] out TValue result,
            [NotNullWhen(false)] out string? validationErrorMessage)
        {
            result = default;
            validationErrorMessage = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                if (Nullable.GetUnderlyingType(typeof(TValue)) != null)
                {
                    result = default!;
                    return true;
                }
                validationErrorMessage = "Enter a time value.";
                return false;
            }
            if (!TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var duration) || duration < TimeSpan.Zero)
            {
                validationErrorMessage = "Enter a valid time value.";
                return false;
            }
            try
            {
                Type type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
                object parsed = type == typeof(TimeSpan) ? duration
                    : type == typeof(TimeOnly) ? TimeOnly.FromTimeSpan(duration)
                    : type == typeof(uint) ? checked((uint)(duration.Ticks / UnitTicks))
                    : checked((int)(duration.Ticks / UnitTicks));
                result = (TValue)parsed;
                return true;
            }
            catch (Exception ex) when (ex is OverflowException || ex is ArgumentOutOfRangeException)
            {
                validationErrorMessage = "The time value is too large.";
                return false;
            }
        }
    }
}
