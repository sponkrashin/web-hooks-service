using WebHooksService;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("web-hooks.json", true);
builder.Services.AddScoped<WebHooksMiddleware>();

var app = builder.Build();
app.UseMiddleware<WebHooksMiddleware>();
app.Run();
