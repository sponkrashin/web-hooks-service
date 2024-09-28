namespace WebHooksService;

using System.Diagnostics;
using System.Text;
using System.Text.Json;

using Microsoft.Net.Http.Headers;

public class WebHooksMiddleware : IMiddleware
{
    private readonly IConfiguration configuration;
    private readonly ILogger<WebHooksMiddleware> logger;

    public WebHooksMiddleware(IConfiguration configuration, ILogger<WebHooksMiddleware> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value!;
        if (path.StartsWith('/') && path.Length != 1)
        {
            path = path[1..];
        }

        if (path == "__web-hooks-config__")
        {
            var stringBuilder = new StringBuilder();

            var webHooksFileConfiguration = this.configuration.GetSection("WebHooksFile").Value;

            stringBuilder.AppendLine($"WebHooksFile: {webHooksFileConfiguration}");

            foreach (var child in this.configuration.GetSection("WebHooks").GetChildren())
            {
                var value = JsonSerializer.Serialize(child.Get<IEnumerable<string>>());
                stringBuilder.AppendLine($"{child.Key}: {value}");
            }

            context.Response.Headers[HeaderNames.ContentType] = "text/plain";
            context.Response.StatusCode = StatusCodes.Status200OK;

            await context.Response.WriteAsync(stringBuilder.ToString());

            return;
        }

        this.logger.LogDebug("Path: {Path}", path);

        var webHookConfigurationSectionName = $"WebHooks:{path}";

        this.logger.LogDebug("Web hook configuration section name: {SectionName}", webHookConfigurationSectionName);

        var webHookConfiguration = this.configuration.GetSection(webHookConfigurationSectionName);
        if (!webHookConfiguration.Exists())
        {
            this.logger.LogInformation("Configuration for web hook with name {WebHookName} does not exist", path);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        this.logger.LogInformation("Configuration for web hook with name {WebHookName} exists", path);

        try
        {
            var webHookCommands = webHookConfiguration.Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();

            this.logger.LogDebug("Web hook commands: {@WebHookCommands}", webHookCommands);

            this.logger.LogInformation("Environment: {Environment}", OperatingSystem.IsWindows() ? "Windows" : "Linux");

            string command;
            string commandArgs;

            if (OperatingSystem.IsWindows())
            {
                command = "cmd";
                commandArgs = $"/C \"{string.Join(" && ", webHookCommands)}\"";
            }
            else
            {
                command = "bash";
                commandArgs = $"-c \"{string.Join(";", webHookCommands)}\"";
            }

            this.logger.LogDebug("Running command {Command} {@Arguments}", command, commandArgs);

            var processStartInfo = new ProcessStartInfo(command, commandArgs);

            var process = Process.Start(processStartInfo);

            this.logger.LogInformation("Process has successfully started");

            await process!.WaitForExitAsync();

            this.logger.LogInformation("Process has successfully finished");

            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred during the request");
            await context.Response.WriteAsync(ex.ToString());
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
