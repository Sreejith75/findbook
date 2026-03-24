namespace FindBook.Infrastructure.Data;

public static class SeedData
{
  public static Task InitializeAsync(AppDbContext dbContext) => Task.CompletedTask;

  public static Task PopulateTestDataAsync(AppDbContext dbContext) => Task.CompletedTask;
}
