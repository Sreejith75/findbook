using FindBook.Infrastructure.Data;

namespace FindBook.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
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

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
