using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace client_sandbox.Client.Components
{
    public partial class CultureChooser : ComponentBase
    {
        private protected readonly CultureInfo[] SupportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("bg-BG")
        };

        [Inject]
        private protected NavigationManager NavigationManager { get; set; }

        [Inject]
        private protected IJSRuntime JSRuntime { get; set; }

        // based on https://github.com/pranavkm/LocSample
        private protected CultureInfo Culture
        {
            get => CultureInfo.CurrentUICulture;
            set
            {
                if (CultureInfo.CurrentUICulture != value)
                {
                    var jsRuntime = (IJSInProcessRuntime)JSRuntime;
                    jsRuntime.InvokeVoid("blazorCulture.set", value.Name);

                    NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
                }
            }
        }
    }
}
