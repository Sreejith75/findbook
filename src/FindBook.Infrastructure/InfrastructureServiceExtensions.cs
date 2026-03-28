using FindBook.Infrastructure.Auth;
using FindBook.Infrastructure.Data;
using Microsoft.Extensions.Options;

namespace FindBook.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger,
    string contentRootPath)
  {
    // Connection string priority:
    // 1. "cleanarchitecture" — injected by .NET Aspire via .WithReference(postgresDb)
    // 2. "DefaultConnection" — manual PostgreSQL connection string (e.g. appsettings.json)
    string? connectionString =
      config.GetConnectionString("cleanarchitecture")
      ?? config.GetConnectionString("DefaultConnection");

    Guard.Against.Null(connectionString, message:
      "A PostgreSQL connection string must be provided via 'cleanarchitecture' (Aspire) or 'DefaultConnection'.");

    services.AddScoped<EventDispatchInterceptor>();
    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

    services.AddDbContext<AppDbContext>((provider, options) =>
    {
      var eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();

      // PostgreSQL via Npgsql
      options.UseNpgsql(connectionString, npgsql =>
      {
        npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
      });

      options.AddInterceptors(eventDispatchInterceptor);
    });

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

    services.Configure<FirebaseAuthOptions>(config.GetSection(FirebaseAuthOptions.SectionName));
    services.AddSingleton(new FirebaseAdminStartupMarker(contentRootPath));
    services.AddSingleton(provider =>
    {
      var marker = provider.GetRequiredService<FirebaseAdminStartupMarker>();
      var startup = new FirebaseAdminStartup(
        provider.GetRequiredService<IOptions<FirebaseAuthOptions>>(),
        provider.GetRequiredService<ILogger<FirebaseAdminStartup>>(),
        marker.ContentRootPath);

      return startup.Initialize();
    });

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }

  private sealed record FirebaseAdminStartupMarker(string ContentRootPath);
}
