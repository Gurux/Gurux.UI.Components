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
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gurux.UI.Components
{
    /// <summary>
    /// Renders a file input restricted to the configured file types.
    /// </summary>
    public class GXInputFile : InputFile
    {
        private EventCallback<InputFileChangeEventArgs> _selectionCallback;
        private EventCallback<InputFileChangeEventArgs> _consumerCallback;

        [CascadingParameter]
        private EditContext? EditContext { get; set; }

        /// <summary>
        /// Optional model field to notify after OnChange completes, for example
        /// ValueExpression="@(() => model.File)". OnChange remains responsible
        /// for assigning the selected file to the model.
        /// </summary>
        [Parameter]
        public Expression<Func<object?>>? ValueExpression { get; set; }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            // Parameters can be supplied again or omitted on a later render.
            // Never capture our own wrapper as the consumer callback.
            if (!OnChange.Equals(_selectionCallback))
            {
                _consumerCallback = OnChange;
            }
            _selectionCallback = EventCallback.Factory.Create<InputFileChangeEventArgs>(this, OnSelectionChangedAsync);
            OnChange = _selectionCallback;
        }

        private async Task OnSelectionChangedAsync(InputFileChangeEventArgs args)
        {
            var context = EditContext;
            FieldIdentifier? field = ValueExpression is null ? null : FieldIdentifier.Create(ValueExpression);
            await _consumerCallback.InvokeAsync(args);
            if (context is not null && field.HasValue)
            {
                context.NotifyFieldChanged(field.Value);
            }
        }

        /// <summary>
        /// Gets or sets the accepted file types passed to the file input.
        /// </summary>
        [Parameter]
        public string? Filter { get; set; }

        /// <summary>
        /// Builds the component's HTML elements, attributes, and event handlers.
        /// </summary>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "type", "file");
            builder.AddAttribute(3, "class", GXComponentAttributes.GetClass(AdditionalAttributes, "form-control"));
            if (!string.IsNullOrEmpty(Filter))
            {
                builder.AddAttribute(4, "accept", Filter);
            }
            builder.AddElementReferenceCapture(5, elementReference => Element = elementReference);
            builder.CloseElement();
        }
    }
}
