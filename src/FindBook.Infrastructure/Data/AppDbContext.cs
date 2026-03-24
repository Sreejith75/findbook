using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
  public DbSet<Library> Libraries => Set<Library>();
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<Book> Books => Set<Book>();
  public DbSet<Rental> Rentals => Set<Rental>();
  public DbSet<DeliveryTask> DeliveryTasks => Set<DeliveryTask>();
  public DbSet<BookReview> BookReviews => Set<BookReview>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override int SaveChanges() =>
        SaveChangesAsync().GetAwaiter().GetResult();
}
