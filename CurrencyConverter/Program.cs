using System;
using CurrencyConverter.ConverterService;
using CurrencyConverter.Data;
using CurrencyConverter.HealthCheck;
using CurrencyConverter.IConverterService;
using CurrencyConverter.MiddelWare;
using CurrencyConverter.Model.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Country Currency API", Version = "v1" });
            });

            // Configuration
            builder.Services.Configure<ExternalApiConfig>(
                builder.Configuration.GetSection("ExternalApis"));

            // MySQL Database
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // HTTP Client with retry policies
            builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "CountryCurrencyApi/1.0");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

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

            // Health Checks - CHOOSE ONE OPTION BELOW:

            // OPTION 1: Use only your custom DatabaseHealthCheck (Recommended)
            builder.Services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database");

            // OPTION 2: If you want to use AddMySql, install the required package first
            // builder.Services.AddHealthChecks()
            //     .AddMySql(connectionString);

            // OPTION 3: Basic health check (no database check)
            // builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Country Currency API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseCors("AllowAll");
            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            // Initialize database
            await InitializeDatabaseAsync(app);

            app.Run();
        }

        private static async Task InitializeDatabaseAsync(WebApplication webApp)
        {
            using var scope = webApp.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Apply pending migrations
                await context.Database.MigrateAsync();

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("MySQL database initialized successfully");
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database");

                // For production, you might want to exit here
                if (webApp.Environment.IsProduction())
                {
                    throw;
                }
            }
        }
    }
}