using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.Shared.Services;
using Playground.Shared.RemoteServices;
using System.Globalization;
using Telerik.Blazor.Services;
using Playground.Shared.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using server_sandbox.Services;
using Playground.Shared.DbServices;
using Playground.Shared.Services.FileManager;
using Telerik.FontIcons;
using Telerik.Blazor;
using Telerik.Blazor.Components;

namespace server_sandbox
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            #region Localization

            services.AddControllers();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>()
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE"),
                    new CultureInfo("es-ES"),
                    new CultureInfo("bg-BG"),
                };

                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            #endregion

            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = $"{Environment.ContentRootPath}/adventure-works.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            services.AddDbContext<AdventureContext>(options => options.UseSqlite(connection));

            // Use this if you have a localdb instance of the Database
            // services.AddDbContext<AdventureContext>(options => options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AdventureWorksLT2012;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"));

            services.AddRazorPages();
            services.AddServerSideBlazor().AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 4 * 1024 * 1024; // 4MB
            });
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<AppointmentService>();
            services.AddSingleton<RecurringAppointmentService>();
            services.AddSingleton<ProductService>();
            services.AddTransient<PersonService>();
            services.AddSingleton<TreeListService>();
            services.AddScoped<DbProductService>();
            services.AddSingleton<ResourceService>();
            services.AddSingleton<AppData>();
            services.AddScoped<LocalStorage>();
            services.AddSingleton<IUploadService, UploadService>();
            services.AddSingleton<FlatFileService>();
            services.AddSingleton<HierarchicalFileService>();
            services.AddSingleton<BaseAppUrlsService>();
            services.AddSingleton<UrlsService>();
            services.AddSingleton<VirtualColumnDataService>();
            services.AddSingleton<StockDataService>();
            services.AddTransient<TreeViewFlatDataService>();
            services.AddTransient<TreeViewHierarchicalDataService>();
            services.AddTransient<TreeViewObservableFlatDataService>();
            services.AddTransient<TreeViewObservableHierarchicalDataService>();

            services.AddTelerikBlazor();

            services.AddSingleton(typeof(ITelerikStringLocalizer), typeof(CustomStringLocalizer));

            // necessary for caching files in FileManager controller
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("JSDebug"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // necessary for caching files in FileManager controller
            app.UseSession();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
