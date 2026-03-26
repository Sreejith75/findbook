using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FindBook.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    var configuration = BuildConfiguration();
    var connectionString =
      Environment.GetEnvironmentVariable("ConnectionStrings__cleanarchitecture")
      ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
      ?? configuration.GetConnectionString("cleanarchitecture")
      ?? configuration.GetConnectionString("DefaultConnection");

    Guard.Against.NullOrWhiteSpace(connectionString,
      message: "A PostgreSQL connection string must be provided for design-time EF operations.");

    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

    optionsBuilder.UseNpgsql(connectionString, npgsql =>
    {
      npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
    });

    return new AppDbContext(optionsBuilder.Options);
  }

  private static IConfiguration BuildConfiguration()
  {
    var configurationRoot = FindWebProjectDirectory();

    return new ConfigurationBuilder()
      .AddJsonFile(Path.Combine(configurationRoot, "appsettings.json"), optional: false)
      .AddJsonFile(Path.Combine(configurationRoot, "appsettings.Development.json"), optional: true)
      .Build();
  }

  private static string FindWebProjectDirectory()
  {
    var current = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (current is not null)
    {
      var directWebProject = Path.Combine(current.FullName, "FindBook.Web.csproj");
      if (File.Exists(directWebProject))
      {
        return current.FullName;
      }

      var nestedWebProject = Path.Combine(current.FullName, "src", "FindBook.Web", "FindBook.Web.csproj");
      if (File.Exists(nestedWebProject))
      {
        return Path.GetDirectoryName(nestedWebProject)!;
      }

      current = current.Parent;
    }

    throw new InvalidOperationException("Could not locate the FindBook.Web project directory for design-time EF configuration.");
  }
}
