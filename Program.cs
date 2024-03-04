using easyurl.Components;
using easyurl.Data;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
var hosts = Environment.GetEnvironmentVariable("ALLOWED_HOST")?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
if (hosts == null || hosts.Length == 0)
{
    hosts = new[] { "localhost" };
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
   options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
   options.AllowedHosts = hosts;
});

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITableStorage,TableStorage>();

var app = builder.Build();
app.UseHostFiltering();
app.UseForwardedHeaders();
app.UseHttpLogging();

app.Use(async (context, next) =>
{
    // Connection: RemoteIp
    app.Logger.LogInformation("Request RemoteIp: {RemoteIpAddress}",
        context.Connection.RemoteIpAddress);

    await next(context);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
