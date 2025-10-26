using System;
using CurrencyConverter.ConverterService;
using CurrencyConverter.Data;
using CurrencyConverter.HealthCheck;
using CurrencyConverter.IConverterService;
using CurrencyConverter.MiddelWare;
using CurrencyConverter.Model.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services.Configure<ExternalApiConfig>(
    builder.Configuration.GetSection("ExternalApis"));

// MySQL Database - Using your Railway connection string
var connectionString = GetConnectionString();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// HTTP Client
builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>();

// Services
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
builder.Services.AddScoped<IRefreshService, RefreshService>();
builder.Services.AddScoped<IImageService, ImageService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Memory Cache
builder.Services.AddMemoryCache();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Initialize database
InitializeDatabase(app);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

static string GetConnectionString()
{
    // Use your Railway connection string
    var railwayUrl = Environment.GetEnvironmentVariable("MYSQL_URL")
                  ?? "mysql://root:alzmuWvxyNKRnudpPoiMbFMvoJioIHhH@mysql.railway.internal:3306/railway";

    return ConvertRailwayConnectionString(railwayUrl);
}

static string ConvertRailwayConnectionString(string railwayUrl)
{
    if (railwayUrl.StartsWith("mysql://"))
    {
        railwayUrl = railwayUrl.Substring(8);
    }

    var parts = railwayUrl.Split('@');
    if (parts.Length != 2)
        return railwayUrl;

    var userPass = parts[0].Split(':');
    var hostDb = parts[1].Split('/');
    var hostPort = hostDb[0].Split(':');

    if (userPass.Length != 2 || hostDb.Length != 2 || hostPort.Length != 2)
        return railwayUrl;

    var user = userPass[0];
    var password = userPass[1];
    var host = hostPort[0];
    var port = hostPort[1];
    var database = hostDb[1];

    return $"Server={host};Database={database};Uid={user};Pwd={password};Port={port};";
}

static void InitializeDatabase(WebApplication webApp)
{
    using var scope = webApp.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database initialized successfully");

        // Log the database connection info (without password)
        logger.LogInformation($"Connected to database: {context.Database.GetDbConnection().Database} on server: {context.Database.GetDbConnection().DataSource}");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed");
    }
}