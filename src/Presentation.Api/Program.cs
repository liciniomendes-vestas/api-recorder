using System.Text.Json;

using Presentation.Api;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var ignoredRoutes = new[]
{
    "/api/FullReleasePackages",
    "/api/ReleasePackages",
    "/api/RemoteBackups",
    "/api/TargetDataPackages",
};

bool IsIgnoredRoute(string route)
{
    for (var i = 0; i < ignoredRoutes.Length; i++)
    {
        if (ignoredRoutes[i].StartsWith(route, StringComparison.InvariantCultureIgnoreCase)) return true;
    }

    return false;
}

var storage = new Storage();
await storage.InitializeAsync();

app.Use(
    async (context, next) =>
    {
        // we don't care about this route
        if (IsIgnoredRoute(context.Request.Path)) return; 
            
        context.Request.EnableBuffering(1024 * 300);
        
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        var request = new Request(
            context.Request.Method,
            context.Request.Path,
            JsonSerializer.Serialize(context.Request.Headers),
            body
        );

        await storage.AddAsync(request);
        
        context.Request.Body.Position = 0;
        
        await next();
    }
);

app.MapGet("/", () => "This API stores all requests to a database");

app.Run();