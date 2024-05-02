using WebHooksService;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddYamlFile("web-hooks.yaml", true);
builder.Services.AddScoped<WebHooksMiddleware>();

var app = builder.Build();
app.UseMiddleware<WebHooksMiddleware>();
app.Run();
