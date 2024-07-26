using Microsoft.AspNetCore.HttpOverrides;

using WebHooksService;

var builder = WebApplication.CreateBuilder(args);

var webHooksFileSetting = builder.Configuration["WebHooksFile"];
if (!string.IsNullOrEmpty(webHooksFileSetting))
{
    builder.Configuration.AddJsonFile(webHooksFileSetting, true);
}

builder.Services.AddScoped<WebHooksMiddleware>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapShortCircuit(404, "favicon.ico");
app.UseMiddleware<WebHooksMiddleware>();

app.Run();
