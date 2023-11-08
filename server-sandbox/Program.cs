using Microsoft.AspNetCore.Localization;
using Microsoft.Data.Sqlite;
using Playground.Shared.DataAccess;
using Playground.Shared.DbServices;
using Playground.Shared.RemoteServices;
using Playground.Shared.Services.FileManager;
using Playground.Shared.Services;
using server_sandbox;
using server_sandbox.Pages;
using server_sandbox.Services;
using System.Globalization;
using Telerik.Blazor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

#region Localization

builder.Services.AddControllers();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
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

var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = $"{builder.Environment.ContentRootPath}/adventure-works.db" };
var connectionString = connectionStringBuilder.ToString();
var connection = new SqliteConnection(connectionString);
builder.Services.AddDbContext<AdventureContext>(options => options.UseSqlite(connection));

// Use this if you have a localdb instance of the Database
// services.AddDbContext<AdventureContext>(options => options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AdventureWorksLT2012;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = 4 * 1024 * 1024; // 4MB
    });
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<AppointmentService>();
builder.Services.AddSingleton<RecurringAppointmentService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddTransient<PersonService>();
builder.Services.AddSingleton<TreeListService>();
builder.Services.AddScoped<DbProductService>();
builder.Services.AddSingleton<ResourceService>();
builder.Services.AddSingleton<AppData>();
builder.Services.AddScoped<LocalStorage>();
builder.Services.AddSingleton<IUploadService, UploadService>();
builder.Services.AddSingleton<FlatFileService>();
builder.Services.AddSingleton<HierarchicalFileService>();
builder.Services.AddSingleton<BaseAppUrlsService>();
builder.Services.AddSingleton<UrlsService>();
builder.Services.AddSingleton<VirtualColumnDataService>();
builder.Services.AddSingleton<StockDataService>();
builder.Services.AddTransient<TreeViewFlatDataService>();
builder.Services.AddTransient<TreeViewHierarchicalDataService>();
builder.Services.AddTransient<TreeViewObservableFlatDataService>();
builder.Services.AddTransient<TreeViewObservableHierarchicalDataService>();


builder.Services.AddTelerikBlazor();

builder.Services.AddSingleton(typeof(ITelerikStringLocalizer), typeof(CustomStringLocalizer));

var app = builder.Build();

if (!app.Environment.IsDevelopment() || !app.Environment.IsEnvironment("JSDebug"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();