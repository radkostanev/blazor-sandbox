using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Playground.Shared.Services;
using System.Net.Http;
using System.Globalization;
using Microsoft.JSInterop;
using Telerik.Blazor.Services;
using Playground.Shared.Services.FileManager;

namespace client_sandbox.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            builder.Services
                .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                .AddTelerikBlazor()
                // inject localization service after the call to .AddTelerikBlazor()
                .AddSingleton(typeof(ITelerikStringLocalizer), typeof(CustomStringLocalizer))
                .AddSingleton<WeatherForecastService>()
                .AddSingleton<AppointmentService>()
                .AddSingleton<RecurringAppointmentService>()
                .AddSingleton<ProductService>()
                .AddSingleton<TreeListService>()
                .AddSingleton<ResourceService>()
                .AddSingleton<AppData>()
                .AddSingleton<VirtualColumnDataService>()
                .AddSingleton<StockDataService>()
                .AddScoped<LocalStorage>()
                .AddTransient<PersonService>()
                .AddTransient<TreeViewFlatDataService>()
                .AddTransient<TreeViewHierarchicalDataService>()
                .AddTransient<TreeViewObservableFlatDataService>()
                .AddTransient<TreeViewObservableHierarchicalDataService>()
                .AddTransient<FlatFileService>()
                .AddTransient<HierarchicalFileService>();

            var host = builder.Build();

            // .NET 6.0 Preview 7 introduced a bug with setting the culture
            // https://github.com/dotnet/aspnetcore/issues/35550
            await SetCultureAsync(host);

            await host.RunAsync();
        }

        // based on https://github.com/pranavkm/LocSample
        private static async Task SetCultureAsync(WebAssemblyHost host)
        {
            var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
            var url = await jsRuntime.InvokeAsync<string>("getPageUrl");
            var isLocalizationDemo =
                url?.ToLowerInvariant().Contains("localization") == true ||
                url?.ToLowerInvariant().Contains("globalization") == true;

            var culture = CultureInfo.InvariantCulture;

            if (isLocalizationDemo)
            {
                // use a hard-coded culture for testing
                // to see if the custom localization messages are applied
                culture = new CultureInfo("bg-BG");
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
