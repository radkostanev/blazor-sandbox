using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Playground.Shared.DataAccess;
using Playground.Shared.Services;
using Playground.Shared.RemoteServices;
using System.Linq;
using Playground.Shared.Services.FileManager;

namespace client_sandbox.Server
{
    public class Startup
    {

        public Startup(IHostEnvironment environment)
        {
            Environment = environment;
        }

        public IHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = $"{Environment.ContentRootFileProvider}/adventure-works.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            services.AddDbContext<AdventureContext>(options => options.UseSqlite(connection));

            services.AddMvc().AddNewtonsoftJson();
            services.AddSingleton<ProductService>();
            services.AddSingleton<IUploadService, UploadService>();
            services.AddSingleton<BaseAppUrlsService>();
            services.AddSingleton<StockDataService>();

            // necessary for caching files in FileManager controller
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var isJsDebugEnvironment = env.IsEnvironment("JSDebug");

            if (env.IsDevelopment() || isJsDebugEnvironment)
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles();

            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod());

            // necessary for caching files in FileManager controller
            app.UseSession();

            app.UseRouting();

            if (isJsDebugEnvironment)
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("development_index.html");
                });
            }
            else
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("index.html");
                });
            }
        }
    }
}
