namespace WebHooksService;

public class WebHooksMiddleware : IMiddleware
{
    private readonly IConfiguration configuration;

    public WebHooksMiddleware(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value!;
        if (path.StartsWith('/') && path.Length != 1)
        {
            path = path[1..];
        }

        var webHookConfiguration = this.configuration.GetSection($"web-hooks:{path}");
        if (!webHookConfiguration.Exists())
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        context.Response.StatusCode = StatusCodes.Status202Accepted;
        return Task.CompletedTask;
    }
}
