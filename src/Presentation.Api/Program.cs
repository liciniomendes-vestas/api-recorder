using System.Text.Json;

using Presentation.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var ignoredRoutes = new[]
{
    "/hello",
};

Func<string, bool> existInRoutes = s =>
{
    for (var i = 0; i < ignoredRoutes.Length; i++)
    {
        if (ignoredRoutes[i].Equals(s, StringComparison.InvariantCultureIgnoreCase)) return true;
    }

    return false;
};

var storage = new Storage();
await storage.InitializeAsync();

app.Use(
    async (context, next) =>
    {
        // we don't care about this route
        if (existInRoutes(context.Request.Path)) return; 
            
        context.Request.EnableBuffering();
        
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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}