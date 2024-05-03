namespace WebHooksService;

using System.Diagnostics;

public class WebHooksMiddleware : IMiddleware
{
    private readonly IConfiguration configuration;

    public WebHooksMiddleware(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value!;
        if (path.StartsWith('/') && path.Length != 1)
        {
            path = path[1..];
        }

        var webHookConfiguration = this.configuration.GetSection($"WebHooks:{path}");
        if (!webHookConfiguration.Exists())
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        try
        {
            var webHookCommands = webHookConfiguration.Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();

            ProcessStartInfo processStartInfo;

            if (OperatingSystem.IsWindows())
            {
                var processArgs = string.Join(" && ", webHookCommands);
                processStartInfo = new ProcessStartInfo("cmd", $"/C \"{processArgs}\"");
            }
            else
            {
                var processArgs = string.Join(";", webHookCommands);
                processStartInfo = new ProcessStartInfo("bash", $"-c \"{processArgs}\"");
            }

            var process = Process.Start(processStartInfo);
            await process!.WaitForExitAsync();

            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }
        catch (Exception ex)
        {
            await context.Response.WriteAsync(ex.ToString());
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
