using FindBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

const string connectionString = "Host=localhost;Port=5432;Database=findbook_db;Username=postgres;Password=Sreejithalr@1";

var options = new DbContextOptionsBuilder<AppDbContext>()
  .UseNpgsql(connectionString)
  .Options;

await using var dbContext = new AppDbContext(options);

await dbContext.Database.MigrateAsync();
await SeedData.InitializeAsync(dbContext);

Console.WriteLine("FindBook seed completed.");
