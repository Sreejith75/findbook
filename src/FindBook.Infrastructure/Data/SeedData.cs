using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using System.Reflection;

namespace FindBook.Infrastructure.Data;

public static class SeedData
{
  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    if (await dbContext.UserAccounts.AnyAsync() ||
        await dbContext.Libraries.AnyAsync() ||
        await dbContext.Categories.AnyAsync() ||
        await dbContext.Books.AnyAsync())
    {
      return;
    }

    var centralLibrary = new Library(
      LibraryName.From("Downtown Reading Hall"),
      new PostalAddress("12 Market Street", "Bengaluru", "Karnataka", "560001", "India"),
      EmailAddress.From("downtown@findbook.local"),
      PhoneNumber.From("+91-80-4000-1000"));
    SetId(centralLibrary, LibraryId.From(1));

    var westLibrary = new Library(
      LibraryName.From("Westside Community Library"),
      new PostalAddress("88 Lake Road", "Bengaluru", "Karnataka", "560034", "India"),
      EmailAddress.From("westside@findbook.local"),
      PhoneNumber.From("+91-80-4000-2000"));
    SetId(westLibrary, LibraryId.From(2));

    dbContext.Libraries.AddRange(centralLibrary, westLibrary);
    await dbContext.SaveChangesAsync();

    var fiction = new Category(CategoryName.From("Fiction"));
    var technology = new Category(CategoryName.From("Technology"));
    var history = new Category(CategoryName.From("History"));
    SetId(fiction, CategoryId.From(1));
    SetId(technology, CategoryId.From(2));
    SetId(history, CategoryId.From(3));

    dbContext.Categories.AddRange(fiction, technology, history);
    await dbContext.SaveChangesAsync();

    var admin = new UserAccount(
      PersonName.From("Akhil Varma"),
      EmailAddress.From("admin@findbook.local"),
      PasswordHash.From("hash-admin"),
      AccountRole.Admin,
      PhoneNumber.From("+91-90000-10001"));
    SetId(admin, UserAccountId.From(1));
    admin.AssignManagedLibrary(centralLibrary.Id);
    admin.AddAddress(SavedAddressId.From(1), new PostalAddress("9 Residency Road", "Bengaluru", "Karnataka", "560025", "India"), true);

    var reader = new UserAccount(
      PersonName.From("Meera Nair"),
      EmailAddress.From("meera@findbook.local"),
      PasswordHash.From("hash-meera"),
      AccountRole.User,
      PhoneNumber.From("+91-90000-10002"));
    SetId(reader, UserAccountId.From(2));
    reader.AddAddress(SavedAddressId.From(2), new PostalAddress("17 Green Park", "Bengaluru", "Karnataka", "560076", "India"), true);

    var deliveryPartner = new UserAccount(
      PersonName.From("Ravi Kumar"),
      EmailAddress.From("delivery@findbook.local"),
      PasswordHash.From("hash-ravi"),
      AccountRole.DeliveryPartner,
      PhoneNumber.From("+91-90000-10003"));
    SetId(deliveryPartner, UserAccountId.From(3));
    deliveryPartner.AddAddress(SavedAddressId.From(3), new PostalAddress("21 Service Lane", "Bengaluru", "Karnataka", "560095", "India"), true);

    dbContext.UserAccounts.AddRange(admin, reader, deliveryPartner);
    await dbContext.SaveChangesAsync();

    var cleanCode = new Book(
      centralLibrary.Id,
      technology.Id,
      BookTitle.From("Clean Code"),
      AuthorName.From("Robert C. Martin"),
      Isbn.From("978-0132350884"),
      new InventoryState(5, 4),
      BookDescription.From("A handbook of agile software craftsmanship."),
      CoverImageReference.From("/covers/clean-code.jpg"));
    SetId(cleanCode, BookId.From(1));

    var hobbit = new Book(
      centralLibrary.Id,
      fiction.Id,
      BookTitle.From("The Hobbit"),
      AuthorName.From("J.R.R. Tolkien"),
      Isbn.From("978-0547928227"),
      new InventoryState(4, 3),
      BookDescription.From("A fantasy adventure before the events of The Lord of the Rings."),
      CoverImageReference.From("/covers/the-hobbit.jpg"));
    SetId(hobbit, BookId.From(2));

    var sapiens = new Book(
      westLibrary.Id,
      history.Id,
      BookTitle.From("Sapiens"),
      AuthorName.From("Yuval Noah Harari"),
      Isbn.From("978-0062316097"),
      new InventoryState(3, 2),
      BookDescription.From("A brief history of humankind."),
      CoverImageReference.From("/covers/sapiens.jpg"));
    SetId(sapiens, BookId.From(3));

    dbContext.Books.AddRange(cleanCode, hobbit, sapiens);
    await dbContext.SaveChangesAsync();

    var activeRental = Rental.Create(
      reader.Id,
      cleanCode.Id,
      centralLibrary.Id,
      DeliveryAddressSnapshot.From(reader.SavedAddresses.Single(x => x.IsDefault).Address),
      DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-5)));
    SetId(activeRental, RentalId.From(1));
    activeRental.MarkDelivered();
    cleanCode.ReserveCopy();

    var pickupRental = Rental.Create(
      reader.Id,
      hobbit.Id,
      centralLibrary.Id,
      DeliveryAddressSnapshot.From(reader.SavedAddresses.Single(x => x.IsDefault).Address),
      DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-15)));
    SetId(pickupRental, RentalId.From(2));
    pickupRental.MarkDelivered();
    pickupRental.RequestReturn(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)));
    hobbit.ReserveCopy();

    dbContext.Rentals.AddRange(activeRental, pickupRental);
    await dbContext.SaveChangesAsync();

    var deliveryTask = new DeliveryTask(
      activeRental.Id,
      deliveryPartner.Id,
      DeliveryTaskType.Delivery,
      new DeliveryAddressSnapshot(
        activeRental.DeliveryAddress.Street,
        activeRental.DeliveryAddress.City,
        activeRental.DeliveryAddress.State,
        activeRental.DeliveryAddress.PostalCode,
        activeRental.DeliveryAddress.Country),
      DateTimeOffset.UtcNow.AddDays(-5));
    SetId(deliveryTask, DeliveryTaskId.From(1));
    deliveryTask.MarkInTransit();
    deliveryTask.Complete(DateTimeOffset.UtcNow.AddDays(-4));

    var pickupTask = new DeliveryTask(
      pickupRental.Id,
      deliveryPartner.Id,
      DeliveryTaskType.Pickup,
      new DeliveryAddressSnapshot(
        pickupRental.DeliveryAddress.Street,
        pickupRental.DeliveryAddress.City,
        pickupRental.DeliveryAddress.State,
        pickupRental.DeliveryAddress.PostalCode,
        pickupRental.DeliveryAddress.Country),
      DateTimeOffset.UtcNow.AddHours(-6));
    SetId(pickupTask, DeliveryTaskId.From(2));
    pickupTask.MarkInTransit();

    dbContext.DeliveryTasks.AddRange(deliveryTask, pickupTask);

    var review = new BookReview(
      cleanCode.Id,
      reader.Id,
      StarRating.From(5),
      ReviewTitle.From("Excellent practical guide"),
      ReviewContent.From("Clear explanations and habits that translate directly into daily coding work."),
      DateTimeOffset.UtcNow.AddDays(-2));
    SetId(review, BookReviewId.From(1));
    cleanCode.ApplyRating(review.Rating.Value);

    dbContext.BookReviews.Add(review);
    await dbContext.SaveChangesAsync();
  }

  public static Task PopulateTestDataAsync(AppDbContext dbContext) => Task.CompletedTask;

  private static void SetId<TEntity, TId>(TEntity entity, TId id)
  {
    var idProperty = typeof(TEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
      ?? throw new InvalidOperationException($"Entity '{typeof(TEntity).Name}' does not expose an Id property.");

    idProperty.SetValue(entity, id);
  }
}
