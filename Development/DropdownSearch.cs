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
using System.Diagnostics.CodeAnalysis;

namespace Gurux.UI.Components
{
    /// <summary>
    /// A dropdown search component.
    /// </summary>
    public class DropdownSearch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>
        : GXInputDropdown<TValue>
    {
        /// <summary>
        /// If immediate filtering is used, the query is executed when the user presses any key.
        /// If immediate is false, the query is executed when the user presses enter.
        /// </summary>
        [Parameter]
        public bool Immediate { get; set; }

        /// <summary>
        /// Maximum number of displayed items. Defaults to 10; zero displays no items.
        /// </summary>
        [Parameter]
        public int MaxItems { get; set; } = 10;

        /// <summary>
        /// Whether to show items when the search input is empty. Defaults to false.
        /// </summary>
        [Parameter]
        public bool ShowItemsWhenEmpty { get; set; }

        /// <summary>
        /// Display text for a null value. Does not change the bound value.
        /// </summary>
        [Parameter]
        public string? NullText { get; set; }

        private bool _nullTextCleared;
        private bool _clearNullTextInput;
        private int _highlightedIndex = -1;
        private bool _keyboardOpen;
        private bool _dismissed;
        private bool _hasFocus;
        private void HandleFocus()
        {
            if (_hasFocus) return;
            _hasFocus = true;
            _dismissed = false;
            Filter ??= string.Empty;
        }
        private void HandleBlur()
        {
            _hasFocus = false;
            _keyboardOpen = false;
            _highlightedIndex = -1;
            _editing = false;
            Filter = null;
        }
        private bool _editing;
        private TValue? _editOriginal;
        private void BeginEdit()
        {
            if (!_editing) { _editOriginal = Value; _editing = true; }
        }
        private readonly string _listId = "dropdown-" + Guid.NewGuid().ToString("N");

        private void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key is "ArrowDown" or "ArrowUp")
            {
                BeginEdit();
                _dismissed = false;
                _keyboardOpen = true;
                int count = Math.Min(_items.Count, MaxItems);
                if (count > 0)
                    _highlightedIndex = _highlightedIndex < 0
                        ? (e.Key == "ArrowDown" ? 0 : count - 1)
                        : Math.Clamp(_highlightedIndex + (e.Key == "ArrowDown" ? 1 : -1), 0, count - 1);
                return;
            }
            if (e.Key == "Enter")
            {
                if (!_dismissed && _highlightedIndex >= 0 && _highlightedIndex < Math.Min(_items.Count, MaxItems))
                    OnItemSelected(_items[_highlightedIndex]);
                return;
            }
            if (e.Key == "Escape")
            {
                if (_editing && !EqualityComparer<TValue>.Default.Equals(Value, _editOriginal))
                {
                    CurrentValue = _editOriginal;
                    OnSelected.InvokeAsync(_editOriginal);
                }
                _editing = false;
                _nullTextCleared = false;
                _clearNullTextInput = false;
                _cts?.Cancel();
                _dismissed = true;
                _keyboardOpen = false;
                _highlightedIndex = -1;
                Filter = null;
                return;
            }
            if (e.Key == "Backspace" && Value is null && !string.IsNullOrEmpty(NullText)
                && string.IsNullOrEmpty(Filter) && !_nullTextCleared)
            {
                _nullTextCleared = true;
                _clearNullTextInput = true;
                Filter = string.Empty;
                return;
            }
            _clearNullTextInput = false;
        }

        [Inject]
        ILogger<DropdownSearch<TValue>>? Logger { get; set; }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            // Resolve supplied text to the actual item, including when Items loads later.
            // Prefer an exact match when the list contains names differing only by case.
            if (!_editing && ItemsProvider == null && Items != null && Value is string text &&
                !Items.Any(item => EqualityComparer<TValue>.Default.Equals(item, Value)))
            {
                foreach (var item in Items)
                {
                    if (item is string name && string.Equals(name, text, StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentValue = item;
                        break;
                    }
                }
            }
        }


        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (ItemsProvider == null && Items != null)
            {
                _items.Clear();
                if (!string.IsNullOrEmpty(Filter))
                {
                    _items.AddRange(Items.Where(s => (Formatter != null ? Formatter(s) : s?.ToString())?.Contains(Filter, StringComparison.OrdinalIgnoreCase) != false).Take(MaxItems));
                }
                else
                {
                    _items.AddRange(Items.Take(MaxItems));
                }
            }
            else
            {
                if (!_items.Any() && Items?.Any() == true)
                {
                    _items.AddRange(Items.Take(MaxItems));
                }
            }
            bool hasSelectedItem = (ItemsProvider == null ? Items ?? _items : _items)
                .Any(item => EqualityComparer<TValue>.Default.Equals(item, Value));
            if (_highlightedIndex >= Math.Min(_items.Count, MaxItems)) _highlightedIndex = -1;
            bool showMenu = _hasFocus && !_dismissed && (_keyboardOpen || !string.IsNullOrEmpty(Filter) ||
                (!hasSelectedItem && ShowItemsWhenEmpty && (Filter != null || string.IsNullOrEmpty(Value?.ToString()))));
            var seq = 0;
            builder.OpenElement(seq++, "div");
            // Cancel native search-input clearing and form submission synchronously,
            // including when the Blazor event handler runs on the server.
            builder.AddAttribute(seq++, "onfocusin", "if(event.target.tagName === 'INPUT' && !event.target.dataset.dropdownKeys){event.target.dataset.dropdownKeys='true';event.target.addEventListener('keydown',function(e){if(!e.isComposing && ['ArrowDown','ArrowUp','Enter','Escape'].includes(e.key))e.preventDefault();});}");
            builder.OpenElement(seq++, "input");
            builder.AddAttribute(seq++, "placeholder", Properties.Resources.Search);
            builder.AddAttribute(seq++, "type", "search");
            builder.AddMultipleAttributes(seq++, AdditionalAttributes);
            builder.AddAttribute(seq++, "role", "combobox");
            builder.AddAttribute(seq++, "aria-expanded", showMenu && _items.Count > 0 ? "true" : "false");
            builder.AddAttribute(seq++, "aria-controls", _listId);
            builder.AddAttribute(seq++, "aria-autocomplete", "list");
            if (showMenu && _highlightedIndex >= 0)
                builder.AddAttribute(seq++, "aria-activedescendant", $"{_listId}-{_highlightedIndex}");
            if (!string.IsNullOrEmpty(NameAttributeValue))
            {
                builder.AddAttribute(seq++, "name", NameAttributeValue);
            }
            if (!string.IsNullOrEmpty(CssClass))
            {
                builder.AddAttribute(seq++, "class", CssClass);
            }

            if (string.IsNullOrEmpty(Filter))
            {
                if (Value is null && _nullTextCleared)
                {
                    builder.AddAttribute(seq++, "value", string.Empty);
                }
                else if (Value is null && NullText is not null)
                {
                    builder.AddAttribute(seq++, "value", NullText);
                }
                else if (Formatter != null && Value is not null)
                {
                    builder.AddAttribute(seq++, "value", Formatter(Value));
                }
                else
                {
                    builder.AddAttribute(seq++, "value", Value);
                }
            }
            else
            {
                builder.AddAttribute(seq++, "value", Filter);
            }

            builder.SetUpdatesAttributeName("value");

            // Clearing the search also clears the selected value and notifies its binding.
            builder.AddAttribute(seq++, "oninput", EventCallback.Factory.CreateBinder<string?>(this, async v =>
            {
                BeginEdit();
                Filter = _clearNullTextInput ? string.Empty : v ?? string.Empty;
                _highlightedIndex = -1;
                _keyboardOpen = false;
                _dismissed = false;
                _clearNullTextInput = false;
                if (Filter.Length == 0)
                {
                    _cts?.Cancel();
                    CurrentValue = default;
                    await OnSelected.InvokeAsync(Value);
                    if (ItemsProvider != null)
                    {
                        await RefreshDataAsync(false);
                        if (Filter == string.Empty)
                        {
                            Value = default;
                        }
                    }
                }
                else if (ItemsProvider != null)
                {
                    await RefreshDataAsync(true);
                    // A clear may have occurred while the provider was completing.
                    if (Filter == string.Empty)
                    {
                        Value = default;
                    }
                }
            }, Filter));
            builder.AddAttribute(seq++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDown));

            builder.AddAttribute(seq++, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, _ => HandleFocus()));
            builder.AddAttribute(seq++, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, _ => HandleBlur()));

            // Capture the element reference for potential focus handling.
            builder.AddElementReferenceCapture(seq++, __inputRef => Element = __inputRef);

            builder.CloseElement();
            if (showMenu)
            {
                if (_items.Any())
                {
                    builder.OpenElement(seq++, "div");
                    builder.AddAttribute(seq++, "class", "dropdown-menu show");
                    builder.AddAttribute(seq++, "id", _listId);
                    builder.AddAttribute(seq++, "role", "listbox");
                    int pos = 0;
                    foreach (var item in _items.Take(MaxItems))
                    {
                        builder.OpenElement(seq++, "a");
                        if (pos == _highlightedIndex || _highlightedIndex < 0 && item?.Equals(Value) == true)
                        {
                            builder.AddAttribute(seq++, "class", "dropdown-item active");
                        }
                        else
                        {
                            builder.AddAttribute(seq++, "class", "dropdown-item");
                        }
                        // Keep focus on the combobox until the click commits the option.
                        // Keyboard selection uses Arrow keys and Enter through aria-activedescendant.
                        builder.AddAttribute(seq++, "tabindex", "-1");
                        builder.AddEventPreventDefaultAttribute(seq++, "onpointerdown", true);
                        builder.AddAttribute(seq++, "id", $"{_listId}-{pos}");
                        builder.AddAttribute(seq++, "role", "option");
                        builder.AddAttribute(seq++, "aria-selected", pos == _highlightedIndex ? "true" : "false");
                        builder.AddAttribute(seq++, "value", pos.ToString());
                        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs?>(this, v => OnItemSelected(item)));
                        if (Template != null)
                        {
                            builder.AddContent(seq++, Template(item));
                        }
                        else
                        {
                            builder.AddContent(seq++, item is null && NullText is not null ? NullText : (object?)item);
                        }
                        builder.CloseElement(); // </dropdown-item>
                        ++pos;
                    }
                    builder.CloseElement(); // </dropdown-menu>
                }
            }
            builder.CloseElement(); // </dropdown>         
        }

        /// <summary>
        /// User has select the new item.
        /// </summary>
        /// <param name="e"></param>
        private void OnItemSelected(TValue? e)
        {
            _editing = false;
            _highlightedIndex = -1;
            _keyboardOpen = false;
            _dismissed = true;
            _nullTextCleared = false;
            CurrentValue = e;
            OnSelected.InvokeAsync(e);
            Filter = null;
        }

        /// <summary>
        /// Refresh search values.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            if (ItemsProvider != null)
            {
                await RefreshDataAsync(false);
            }
            await base.OnInitializedAsync();
        }
    }
}

