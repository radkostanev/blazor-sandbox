/*
Code copied from https://github.com/Blazored/FluentValidation
Use only for internal use in Playground examples
 */
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Telerik.Blazor.Components.Common;
using FluentValidation;
using Playground.Shared.Extensions;

namespace client_sandbox.Client.Shared
{
    public partial class FluentValidationValidator : ComponentBase
    {
        [CascadingParameter]
        public EditContext CurrentEditContext { get; set; }

        [Parameter]
        public IValidator Validator { get; set; }

        protected override void OnInitialized()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException($"{nameof(FluentValidationValidator)} requires a cascading " +
                    $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(FluentValidationValidator)} " +
                    $"inside an {nameof(EditForm)}.");
            }

            Console.WriteLine(Validator == null);
            CurrentEditContext.AddFluentValidation(Validator);
        }
    }
}
