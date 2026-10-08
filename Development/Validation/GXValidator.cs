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
