var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddYamlFile("web-hooks.yaml", true);

var app = builder.Build();
app.Run();
