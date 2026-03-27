using Serilog;

namespace FindBook.Web.Configurations;

public static class LoggerConfigs
{
  public static WebApplicationBuilder AddLoggerConfigs(this WebApplicationBuilder builder)
  {
    builder.Logging.ClearProviders();

    builder.Host.UseSerilog((context, _, configuration) =>
      configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName));

    return builder;
  }
}
