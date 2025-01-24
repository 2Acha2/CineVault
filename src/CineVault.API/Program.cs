using CineVault.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddCineVaultDbContext(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders(); // Очищення стандартних провайдерів
builder.Logging.AddConsole(options =>
{
    //options.IncludeScopes = true;
    options.LogToStandardErrorThreshold = LogLevel.Warning;
}
);
builder.Logging.AddDebug();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

Console.WriteLine($" Active Environment: {app.Environment.EnvironmentName}");

if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local" || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();