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
using System.Linq.Expressions;

namespace Gurux.UI.Components.Validation
{
    /// <summary>
    /// Validates an edit context using application callbacks and manages field validation messages.
    /// </summary>
    public class GXValidator : ComponentBase
    {
        private ValidationMessageStore? _messageStore;

        /// <summary>
        /// Gets or sets the cascading edit context whose validation messages are managed by this component.
        /// </summary>
        [CascadingParameter]
        private EditContext? CurrentEditContext { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous callback used to validate the current edit context.
        /// </summary>
        [Parameter]
        public EventCallback<GXValidator> OnValidate { get; set; }

        /// <summary>
        /// Verifies the cascading edit context and subscribes to validation requests.
        /// </summary>
        protected override void OnInitialized()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException(
                                $"{nameof(GXValidator)} requires a cascading " +
                                $"parameter of type {nameof(EditContext)}.");
            }

            _messageStore = new ValidationMessageStore(CurrentEditContext);

            CurrentEditContext.OnValidationRequested += (s, e) =>
                _messageStore.Clear();
            CurrentEditContext.OnFieldChanged += (s, e) =>
                _messageStore.Clear(e.FieldIdentifier);
        }

        /// <summary>
        /// Adds a validation message for the specified model field.
        /// </summary>
        /// <param name="accessor">Field identifier.</param>
        /// <param name="message">Error message.</param>
        public void AddError(Expression<Func<object?>> accessor, string message)
        {
            _messageStore?.Add(FieldIdentifier.Create(accessor), message);
        }

        /// <summary>
        /// Adds a validation message for the specified model field.
        /// </summary>
        /// <param name="identier">Field identifier.</param>
        /// <param name="message">Error message.</param>
        public void AddError(string identier, string message)
        {
            if (CurrentEditContext != null)
            {
                _messageStore?.Add(CurrentEditContext.Field(identier), message);
            }
        }

        /// <summary>
        /// Runs application validation and reports whether the current edit context has any validation messages.
        /// </summary>
        /// <returns>True, if content is valid.</returns>
        public async Task<bool> ValidateAsync()
        {
            if (CurrentEditContext != null && _messageStore != null)
            {
                await OnValidate.InvokeAsync(this);
                CurrentEditContext.NotifyValidationStateChanged();
                return !CurrentEditContext.GetValidationMessages().Any();
            }
            return true;
        }

        /// <summary>
        /// Adds the supplied field validation messages and notifies the edit context that validation changed.
        /// </summary>
        /// <param name="errors">The field names and validation messages to display.</param>
        public void DisplayErrors(Dictionary<string, List<string>> errors)
        {
            if (CurrentEditContext != null && _messageStore != null)
            {
                foreach (var err in errors)
                {
                    _messageStore.Add(CurrentEditContext.Field(err.Key), err.Value);
                }
                CurrentEditContext.NotifyValidationStateChanged();
            }
        }

        /// <summary>
        /// Clears field validation messages and notifies the edit context that validation changed.
        /// </summary>
        public void ClearErrors()
        {
            _messageStore?.Clear();
            CurrentEditContext?.NotifyValidationStateChanged();
        }
    }
}
