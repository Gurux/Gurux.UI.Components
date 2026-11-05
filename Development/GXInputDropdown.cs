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
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Gurux.UI.Components
{
    /// <summary>
    /// Renders a typed select input with static or asynchronously provided items and optional value persistence.
    /// </summary>
    public class GXInputDropdown<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue> : InputBase<TValue>
    {
        /// <summary>Gets or sets whether user interaction is disabled.</summary>
        [Parameter]
        public bool Disabled { get; set; }

        /// <summary>Allows standalone selection callbacks without a bound field expression.</summary>
        public override Task SetParametersAsync(ParameterView parameters)
        {
            if (!parameters.TryGetValue<System.Linq.Expressions.Expression<Func<TValue>>>(nameof(ValueExpression), out var expression) && ValueExpression == null)
            {
                ValueExpression = () => Value!;
            }
            return base.SetParametersAsync(parameters);
        }
        /// <summary>
        /// Cancellation token.
        /// </summary>
        protected CancellationTokenSource? _cts;
        /// <summary>
        /// Filter.
        /// </summary>
        protected string? Filter;

        /// <summary>
        /// Gets or sets the content rendered inside the component.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the select element reference, available after rendering.
        /// </summary>
        [DisallowNull]
        public ElementReference? Element { get; protected set; }

        /// <summary>
        /// Converts an item to the display text used by a dropdown.
        /// </summary>
        public delegate string? FormatEventHandler(TValue value);

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
        ILogger<GXInputDropdown<TValue>>? Logger { get; set; }

        /// <summary>
        /// Gets or sets the callback invoked when the user selects an item.
        /// </summary>
        [Parameter]
        public EventCallback<TValue> OnSelected { get; set; }

        /// <summary>
        /// Gets or sets the items available for display or selection.
        /// </summary>
        [Parameter]
        public IEnumerable<TValue>? Items { get; set; }

        /// <summary>
        /// Items in the list.
        /// </summary>
        protected readonly List<TValue> _items = new List<TValue>();


        /// <summary>
        /// Gets or sets the asynchronous provider used to retrieve items and their total count.
        /// </summary>
        [Parameter]
        public GXItemsProviderDelegate<TValue>? ItemsProvider { get; set; }


        /// <summary>
        /// Initializes item loading and restores the saved selection when persistence is enabled.
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }
            try
            {
                if (ItemsProvider != null)
                {
                    await RefreshDataAsync(true);
                }
                //Get the default value from the cookies
                // if it's not set.
                if (localStorage != null && RememberValue && !string.IsNullOrEmpty(Id))
                {
                    string? value = await localStorage.GetValueAsync(Id);
                    if (value != null)
                    {
                        Logger?.LogWarning("Default value for {0} is loaded from local storage '{1}'.", Id, value);
                        if (TryParseValueFromString(value, out TValue? value2, out var _))
                        {
                            bool changed = !EqualityComparer<TValue>.Default.Equals(CurrentValue, value2);
                            CurrentValue = value2;
                            if (changed)
                            {
                                await OnSelected.InvokeAsync(CurrentValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex.Message);
            }
            StateHasChanged();
        }

        /// <summary>
        /// Retrieves items from the provider and optionally requests rendering after a successful load.
        /// </summary>
        /// <param name="renderOnSuccess">Is UI render after success operation.</param>
        public async Task RefreshDataAsync(bool renderOnSuccess = true)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken cancellationToken = _cts.Token;
            GXItemsProviderRequest req = new GXItemsProviderRequest(0, 0,
            true,
            false,
            null,
            false,
            Filter,
            cancellationToken);
            if (ItemsProvider == null)
            {
                throw new Exception("ItemsProvider not set.");
            }
            var result = await ItemsProvider(req);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            _items.Clear();
            if (result.Items != null)
            {
                _items.AddRange(result.Items);
            }
            if ((Value == null || !_items.Contains(Value)) && _items.Any())
            {
                CurrentValue = _items.First();
            }
            // Only apply result if the task was not canceled.
            if (!cancellationToken.IsCancellationRequested && renderOnSuccess)
            {
                StateHasChanged();
            }
        }
        private readonly bool _isMultipleSelect;

        /// <summary>
        /// Detects multiple selection and populates enumeration values, including a null option for nullable enums.
        /// </summary>
        public GXInputDropdown()
        {
            _isMultipleSelect = typeof(TValue).IsArray;
            var enumType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            if (enumType.IsEnum)
            {
                // Nullable enums must allow selecting and restoring null.
                if (Nullable.GetUnderlyingType(typeof(TValue)) != null)
                {
                    _items.Add(default!);
                }
                foreach (TValue it in Enum.GetValues(enumType))
                {
                    _items.Add(it);
                }
            }
        }

        /// <summary>
        /// Builds the component's HTML elements, attributes, and event handlers.
        /// </summary>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Items is not null)
            {
                _items.Clear();
                _items.AddRange(Items);
            }

            builder.OpenElement(0, "select");
            builder.AddAttribute(1, "id", Id);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            if (!string.IsNullOrEmpty(NameAttributeValue))
            {
                builder.AddAttribute(3, "name", NameAttributeValue);
            }
            string? cssClass = GXComponentAttributes.CombineClasses("form-select", CssClass);
            builder.AddAttribute(4, "class", cssClass);
            builder.AddAttribute(5, "multiple", _isMultipleSelect);
            builder.AddAttribute(5, "disabled", Disabled);
            if (_isMultipleSelect)
            {
                builder.AddAttribute(6, "value", BindConverter.FormatValue(CurrentValue)?.ToString());
                builder.AddAttribute(7, "onchange", EventCallback.Factory.CreateBinder<string?[]?>(this, SetCurrentValueAsStringArray, default));
            }
            else
            {
                builder.AddAttribute(8, "value", Value == null ? string.Empty : _items.IndexOf(Value!).ToString(CultureInfo.InvariantCulture));
                builder.AddAttribute(9, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(
                this, async e => await SetCurrentValueAsStringAsync(e.Value)));
            }
            builder.AddAttribute(10, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, OnKeyDown));
            builder.SetUpdatesAttributeName("value");
            builder.AddElementReferenceCapture(11, __selectReference => Element = __selectReference);
            builder.AddContent(12, ChildContent);

            int index = 13;
            int pos = 0;
            foreach (var value in _items)
            {
                builder.OpenElement(index++, "option");
                if (value == null)
                {
                    builder.AddAttribute(index++, "value", string.Empty);
                    // Keep the empty option visible and selectable in native menus.
                    builder.AddContent(index++, "\u00a0");
                }
                else if (Template != null)
                {
                    builder.AddAttribute(index++, "id", pos.ToString());
                    builder.AddAttribute(index++, "value", pos.ToString());
                    builder.AddContent(index++, Template(value));
                }
                else
                {
                    builder.AddAttribute(index++, "id", pos.ToString());
                    builder.AddAttribute(index++, "value", pos.ToString());
                    builder.AddContent(index++, FormatItem(value));
                }
                ++pos;
                builder.CloseElement();
            }
            builder.CloseElement();
        }
        private TValue? _original;
        private bool _hasOriginal;

        /// <summary>
        /// Tracks the original selection during keyboard navigation and handles selection or cancellation keys.
        /// </summary>
        protected void OnKeyDown(KeyboardEventArgs e)
        {
            if (Disabled)
            {
                return;
            }
            if (!_hasOriginal)
            {
                //Remember the selected item.
                _original = Value;
                _hasOriginal = true;
            }
            if (e.Key == "ArrowUp")
            {
                //Select the previos item.
                int index = _items.IndexOf(Value!);
                if (index > 0 && index < _items.Count)
                {
                    CurrentValue = _items[index - 1];
                }
            }
            else if (e.Key == "ArrowDown")
            {
                //Select the next item.
                int index = _items.IndexOf(Value!);
                if (index != -1 && index < _items.Count - 1)
                {
                    CurrentValue = _items[index + 1];
                }
            }
            else if (e.Key == "Enter")
            {
                _original = default!;
                _hasOriginal = false;
                OnSelected.InvokeAsync(Value);
                if (localStorage != null && RememberValue && !string.IsNullOrEmpty(Id))
                {
                    localStorage.SetValueAsync(Id, Value?.ToString());
                }
                Filter = null;
            }
            else if (e.Key == "Escape")
            {
                //Reset to original value.
                Filter = null;
                CurrentValue = _original;
                _original = default!;
                _hasOriginal = false;
                OnSelected.InvokeAsync(Value);
                StateHasChanged();
            }
        }

        /// <summary>
        /// Converts a multiple-selection value to the bound type and clears the original keyboard selection.
        /// </summary>
        protected void SetCurrentValueAsStringArray(string?[]? value)
        {
            _original = default!;
            _hasOriginal = false;
            if (Disabled)
            {
                return;
            }
            if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.CurrentCulture, out var result))
            {
                // Conversion creates a new array even when the selected values are unchanged.
                if (CurrentValue is Array previous && result is Array next &&
                    previous.Cast<object?>().SequenceEqual(next.Cast<object?>()))
                {
                    return;
                }
                CurrentValue = result;
            }
        }

        /// <summary>
        /// Resolves a selected option index, updates the bound item, and invokes the selection callback.
        /// </summary>
        protected async Task SetCurrentValueAsString(object? value)
        {
            _original = default!;
            _hasOriginal = false;
            if (Disabled)
            {
                return;
            }
            int index = value is string text && text.Length == 0
                ? _items.FindIndex(item => item == null)
                : int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : -1;
            if (index < 0 || index >= _items.Count)
            {
                return;
            }
            if (EqualityComparer<TValue>.Default.Equals(CurrentValue, _items[index]))
            {
                return;
            }
            CurrentValue = _items[index];
            await OnSelected.InvokeAsync(Value);
        }
        /// <summary>
        /// Applies the selected option and persists the resulting value when enabled.
        /// </summary>
        private async Task SetCurrentValueAsStringAsync(object? value)
        {
            if (Disabled)
            {
                return;
            }
            await SetCurrentValueAsString(value);
            if (localStorage != null && RememberValue && !string.IsNullOrEmpty(Id))
            {
                if (Value != null)
                {
                    await localStorage.SetValueAsync(Id, Value.ToString());
                }
                else
                {
                    await localStorage.SetValueAsync(Id, string.Empty);
                }
            }
        }

        /// <summary>
        /// Converts persisted text to the bound type without modifying the current selection.
        /// </summary>
        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            foreach (var item in _items)
            {
                if (string.Equals(item?.ToString(), value, StringComparison.Ordinal))
                {
                    result = item;
                    validationErrorMessage = null;
                    return true;
                }
            }
            if (string.IsNullOrEmpty(value) && default(TValue) == null)
            {
                result = default!;
                validationErrorMessage = null;
                return true;
            }
            try
            {
                if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.InvariantCulture, out result))
                {
                    validationErrorMessage = null;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                // Items without a TypeConverter are restored by their stored display text.
            }
            result = default!;
            validationErrorMessage = "Invalid value.";
            return false;
        }

        /// <summary>Formats display text while leaving selection and persisted values unchanged.</summary>
        protected string? FormatItem(TValue? value)
        {
            if (value is null)
            {
                return null;
            }
            return Formatter?.Invoke(value) ?? value.ToString();
        }

        /// <summary>
        /// Gets or sets the function used to convert an item to display text. Returning null uses the item's own text; an empty string is displayed as supplied.
        /// </summary>
        [Parameter]
        public FormatEventHandler? Formatter { get; set; }

        /// <summary>
        /// Gets or sets the template used to render an individual item.
        /// </summary>
        [Parameter]
        public RenderFragment<TValue>? Template { get; set; }

    }
}

