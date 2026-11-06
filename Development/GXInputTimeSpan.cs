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
    /// Edits a duration in one text field, showing whole days automatically when present.
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
        /// Gets or sets the duration input element reference, available after rendering.
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

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "id", Id);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddAttribute(3, "type", "text");
            if (!string.IsNullOrEmpty(NameAttributeValue))
            {
                builder.AddAttribute(4, "name", NameAttributeValue);
            }
            builder.AddAttribute(5, "class", GXComponentAttributes.CombineClasses("form-control", CssClass));
            builder.AddAttribute(6, "value", CurrentValueAsString);
            builder.AddAttribute(7, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnDurationChanged));
            builder.SetUpdatesAttributeName("value");
            if (GXComponentAttributes.GetValue(AdditionalAttributes, "placeholder") == null)
            {
                builder.AddAttribute(8, "placeholder", TimeSpanUnit == TimeSpanUnit.Minute ? "d.hh:mm" : "d.hh:mm:ss");
            }
            builder.AddElementReferenceCapture(9, reference => Element = reference);
            builder.CloseElement();
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

        private Task OnDurationChanged(ChangeEventArgs args)
        {
            return ApplyTextAsync(Convert.ToString(args.Value, CultureInfo.InvariantCulture));
        }

        /// <inheritdoc />
        protected override string? FormatValueAsString(TValue? value)
        {
            if (value is null)
            {
                return null;
            }
            TimeSpan duration = GetDuration(value);
            if (TimeSpanUnit == TimeSpanUnit.Minute && duration.Ticks % TimeSpan.TicksPerMinute == 0)
            {
                return duration.ToString(duration.Days > 0 ? @"d\.hh\:mm" : @"hh\:mm", CultureInfo.InvariantCulture);
            }
            return duration.ToString("c", CultureInfo.InvariantCulture);
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
                validationErrorMessage = "Enter a duration.";
                return false;
            }
            if (!value.Contains(':') || !TimeSpan.TryParseExact(value.Trim(),
                ["c", @"hh\:mm", @"d\.hh\:mm"], CultureInfo.InvariantCulture, out var duration) || duration < TimeSpan.Zero)
            {
                validationErrorMessage = "Enter a duration as hh:mm:ss or d.hh:mm:ss.";
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
                validationErrorMessage = "The duration is too large for this value type.";
                return false;
            }
        }
    }
}
