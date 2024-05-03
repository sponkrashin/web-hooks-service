using Microsoft.AspNetCore.HttpOverrides;

using WebHooksService;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("web-hooks.json", true);
builder.Services.AddScoped<WebHooksMiddleware>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapShortCircuit(404, "favicon.ico");
app.UseMiddleware<WebHooksMiddleware>();

app.Run();
