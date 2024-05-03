using WebHooksService;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("web-hooks.json", true);
builder.Services.AddScoped<WebHooksMiddleware>();

var app = builder.Build();

app.MapShortCircuit(404, "favicon.ico");
app.UseMiddleware<WebHooksMiddleware>();

app.Run();
