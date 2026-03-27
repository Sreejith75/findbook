using FindBook.Web.Admin;
using FindBook.Web.Books;
using FindBook.Web.Categories;
using FindBook.Web.Configurations;
using FindBook.Web.Delivery;
using FindBook.Web.Libraries;
using FindBook.Web.Rentals;
using FindBook.Web.Reviews;
using FindBook.Web.Users;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()    // This sets up OpenTelemetry logging
       .AddLoggerConfigs();     // This adds Serilog for console formatting

using var startupLoggerFactory = LoggerFactory.Create(logging =>
{
  logging.ClearProviders();
  logging.AddSerilog();
});
var startupLogger = startupLoggerFactory.CreateLogger<Program>();

startupLogger.LogInformation("Starting web host");

builder.Services.AddOptionConfigs(builder.Configuration, startupLogger, builder);
builder.Services.AddServiceConfigs(startupLogger, builder);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "FindBook API",
    Version = "v1",
    Description = "HTTP API for the FindBook book rental and delivery platform."
  });
});

var app = builder.Build();

await app.UseAppMiddlewareAndSeedDatabase();

app.MapDefaultEndpoints(); // Aspire health checks and metrics
app.MapUserEndpoints();
app.MapLibraryEndpoints();
app.MapCategoryEndpoints();
app.MapBookEndpoints();
app.MapBookImageEndpoints();
app.MapRentalEndpoints();
app.MapDeliveryTaskEndpoints();
app.MapBookReviewEndpoints();
app.MapAdminEndpoints();

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
