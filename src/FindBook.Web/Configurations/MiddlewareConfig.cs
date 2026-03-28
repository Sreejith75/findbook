using Ardalis.ListStartupServices;
using FindBook.Infrastructure.Data;

namespace FindBook.Web.Configurations;

public static class MiddlewareConfig
{
  public static async Task<IApplicationBuilder> UseAppMiddlewareAndSeedDatabase(this WebApplication app)
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
      app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
      app.UseSwagger();
      app.UseSwaggerUI(options =>
      {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FindBook API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "FindBook API";
      });
    }
    else
    {
      app.UseExceptionHandler(exceptionHandlerApp =>
      {
        exceptionHandlerApp.Run(async context =>
        {
          context.Response.StatusCode = StatusCodes.Status500InternalServerError;
          context.Response.ContentType = "application/json";
          await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
        });
      });
      app.UseHsts();
    }

    app.UseHttpsRedirection(); // Note this will drop Authorization headers
    app.UseAuthentication();
    app.UseAuthorization();

    // Run migrations and seed in Development or when explicitly requested via environment variable
    var shouldMigrate = app.Environment.IsDevelopment() || 
                        app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
    
    if (shouldMigrate)
    {
      await MigrateDatabaseAsync(app);
      await SeedDatabaseAsync(app);
    }

    return app;
  }

  static async Task MigrateDatabaseAsync(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
      logger.LogInformation("Applying database migrations...");
      var context = services.GetRequiredService<AppDbContext>();
      await context.Database.MigrateAsync();
      logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred migrating the DB. {exceptionMessage}", ex.Message);
      throw; // Re-throw to make startup fail if migrations fail
    }
  }

  static async Task SeedDatabaseAsync(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
      logger.LogInformation("Seeding database...");
      var context = services.GetRequiredService<AppDbContext>();
      await SeedData.InitializeAsync(context);
      logger.LogInformation("Database seeded successfully");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
      // Don't re-throw for seeding errors - it's not critical
    }
  }
}
