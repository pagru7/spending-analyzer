using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SpendingAnalyzer.Data;
using FastEndpoints.Swagger;
using Microsoft.Extensions.DependencyInjection;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/spending-analyzer-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting SpendingAnalyzer API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<SpendingAnalyzerDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Add FastEndpoints
    builder.Services.AddFastEndpoints();

    // CORS (to allow browser preflight OPTIONS requests)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // FastEndpoints Swagger document
    builder.Services.SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.DocumentName = "v1";
            s.Title = "SpendingAnalyzer API";
            s.Version = "v1";

        };
    });

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Enable CORS before endpoints
    app.UseCors("AllowAll");

    // Use FastEndpoints
    app.UseFastEndpoints();

    // Enable Swagger UI (FastEndpoints)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerGen();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
