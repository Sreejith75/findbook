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

    var riversideLibrary = new Library(
      LibraryName.From("Riverside Knowledge Hub"),
      new PostalAddress("45 River View Avenue", "Kochi", "Kerala", "682016", "India"),
      EmailAddress.From("riverside@findbook.local"),
      PhoneNumber.From("+91-484-4000-3000"));
    SetId(riversideLibrary, LibraryId.From(3));

    dbContext.Libraries.AddRange(centralLibrary, westLibrary, riversideLibrary);
    await dbContext.SaveChangesAsync();

    var fiction = new Category(CategoryName.From("Fiction"));
    var technology = new Category(CategoryName.From("Technology"));
    var history = new Category(CategoryName.From("History"));
    var science = new Category(CategoryName.From("Science"));
    var personalGrowth = new Category(CategoryName.From("Personal Growth"));
    var business = new Category(CategoryName.From("Business"));
    SetId(fiction, CategoryId.From(1));
    SetId(technology, CategoryId.From(2));
    SetId(history, CategoryId.From(3));
    SetId(science, CategoryId.From(4));
    SetId(personalGrowth, CategoryId.From(5));
    SetId(business, CategoryId.From(6));

    dbContext.Categories.AddRange(fiction, technology, history, science, personalGrowth, business);
    await dbContext.SaveChangesAsync();

    var admin = new UserAccount(
      PersonName.From("Sreejith K"),
      EmailAddress.From("sreejithalr78@gmail.com"),
      PasswordHash.From("testuser@1"),
      AccountRole.Admin,
      PhoneNumber.From("+91-90000-10001"));
    SetId(admin, UserAccountId.From(1));
    admin.AssignManagedLibrary(centralLibrary.Id);
    admin.AddAddress(SavedAddressId.From(1), new PostalAddress("Kakkanad", "Kochi", "Kerala", "560025", "India"), true);

    var reader = new UserAccount(
      PersonName.From("Nimisha Ganesh"),
      EmailAddress.From("nimisha@gmail.com"),
      PasswordHash.From("nimisha@1"),
      AccountRole.User,
      PhoneNumber.From("+91-90000-10002"));
    SetId(reader, UserAccountId.From(2));
    reader.AddAddress(SavedAddressId.From(2), new PostalAddress("17 Green Park", "Bengaluru", "Karnataka", "560076", "India"), true);

    var deliveryPartner = new UserAccount(
      PersonName.From("Sanchez Thomson"),
      EmailAddress.From("sanchez@gmail.com"),
      PasswordHash.From("sanchez@1"),
      AccountRole.DeliveryPartner,
      PhoneNumber.From("+91-90000-10003"));
    SetId(deliveryPartner, UserAccountId.From(3));
    deliveryPartner.AddAddress(SavedAddressId.From(3), new PostalAddress("21 Service Lane", "Bengaluru", "Karnataka", "560095", "India"), true);

    var superAdmin = new UserAccount(
      PersonName.From("Ananya Menon"),
      EmailAddress.From("ananya.superadmin@findbook.local"),
      PasswordHash.From("superadmin@1"),
      AccountRole.SuperAdmin,
      PhoneNumber.From("+91-90000-10004"));
    SetId(superAdmin, UserAccountId.From(4));
    superAdmin.AddAddress(SavedAddressId.From(4), new PostalAddress("14 Queens Walk", "Kochi", "Kerala", "682011", "India"), true);

    var secondReader = new UserAccount(
      PersonName.From("Rahul Nair"),
      EmailAddress.From("rahul.nair@findbook.local"),
      PasswordHash.From("rahul@123"),
      AccountRole.User,
      PhoneNumber.From("+91-90000-10005"));
    SetId(secondReader, UserAccountId.From(5));
    secondReader.AddAddress(SavedAddressId.From(5), new PostalAddress("92 Hill Crest", "Kochi", "Kerala", "682020", "India"), true);

    var secondDeliveryPartner = new UserAccount(
      PersonName.From("Meera Das"),
      EmailAddress.From("meera.dispatch@findbook.local"),
      PasswordHash.From("meera@123"),
      AccountRole.DeliveryPartner,
      PhoneNumber.From("+91-90000-10006"));
    SetId(secondDeliveryPartner, UserAccountId.From(6));
    secondDeliveryPartner.AddAddress(SavedAddressId.From(6), new PostalAddress("7 Transit Point", "Kochi", "Kerala", "682018", "India"), true);

    dbContext.UserAccounts.AddRange(admin, reader, deliveryPartner, superAdmin, secondReader, secondDeliveryPartner);
    await dbContext.SaveChangesAsync();

    var cleanCode = new Book(
      centralLibrary.Id,
      technology.Id,
      BookTitle.From("Clean Code"),
      AuthorName.From("Robert C. Martin"),
      Isbn.From("978-0132350884"),
      new InventoryState(5, 5),
      BookDescription.From("A handbook of agile software craftsmanship."),
      CoverImageReference.From("/covers/clean-code.jpg"));
    SetId(cleanCode, BookId.From(1));

    var pragmaticProgrammer = new Book(
      riversideLibrary.Id,
      technology.Id,
      BookTitle.From("The Pragmatic Programmer"),
      AuthorName.From("Andrew Hunt and David Thomas"),
      Isbn.From("978-0201616224"),
      new InventoryState(5, 5),
      BookDescription.From("A practical guide to sustainable software craftsmanship and engineering habits."),
      CoverImageReference.From("/covers/pragmatic-programmer.jpg"));
    SetId(pragmaticProgrammer, BookId.From(2));

    var refactoring = new Book(
      riversideLibrary.Id,
      technology.Id,
      BookTitle.From("Refactoring"),
      AuthorName.From("Martin Fowler"),
      Isbn.From("978-0201485677"),
      new InventoryState(4, 4),
      BookDescription.From("A foundational book on improving existing code without changing behavior."),
      CoverImageReference.From("/covers/refactoring.jpg"));
    SetId(refactoring, BookId.From(3));

    var designingDataIntensiveApplications = new Book(
      riversideLibrary.Id,
      technology.Id,
      BookTitle.From("Designing Data-Intensive Applications"),
      AuthorName.From("Martin Kleppmann"),
      Isbn.From("978-1491903117"),
      new InventoryState(4, 4),
      BookDescription.From("A modern systems design guide focused on reliable, scalable, and maintainable software."),
      CoverImageReference.From("/covers/ddia.jpg"));
    SetId(designingDataIntensiveApplications, BookId.From(4));

    var atomicHabits = new Book(
      westLibrary.Id,
      personalGrowth.Id,
      BookTitle.From("Atomic Habits"),
      AuthorName.From("James Clear"),
      Isbn.From("978-0593189641"),
      new InventoryState(6, 6),
      BookDescription.From("A practical framework for building better habits through small, repeatable improvements."),
      CoverImageReference.From("/covers/atomic-habits.jpg"));
    SetId(atomicHabits, BookId.From(5));

    var deepWork = new Book(
      westLibrary.Id,
      personalGrowth.Id,
      BookTitle.From("Deep Work"),
      AuthorName.From("Cal Newport"),
      Isbn.From("978-1455586691"),
      new InventoryState(5, 5),
      BookDescription.From("A focused argument for distraction-free concentration in knowledge work."),
      CoverImageReference.From("/covers/deep-work.jpg"));
    SetId(deepWork, BookId.From(6));

    var hobbit = new Book(
      centralLibrary.Id,
      fiction.Id,
      BookTitle.From("The Hobbit"),
      AuthorName.From("J.R.R. Tolkien"),
      Isbn.From("978-0547928227"),
      new InventoryState(4, 4),
      BookDescription.From("A classic fantasy quest that follows Bilbo Baggins across Middle-earth."),
      CoverImageReference.From("/covers/the-hobbit.jpg"));
    SetId(hobbit, BookId.From(7));

    var dune = new Book(
      centralLibrary.Id,
      fiction.Id,
      BookTitle.From("Dune"),
      AuthorName.From("Frank Herbert"),
      Isbn.From("978-0441172719"),
      new InventoryState(4, 4),
      BookDescription.From("A science-fiction epic set on Arrakis, where politics, prophecy, and survival collide."),
      CoverImageReference.From("/covers/dune.jpg"));
    SetId(dune, BookId.From(8));

    var nineteenEightyFour = new Book(
      westLibrary.Id,
      fiction.Id,
      BookTitle.From("1984"),
      AuthorName.From("George Orwell"),
      Isbn.From("978-0451524935"),
      new InventoryState(5, 5),
      BookDescription.From("A dystopian novel about surveillance, truth, and authoritarian control."),
      CoverImageReference.From("/covers/1984.jpg"));
    SetId(nineteenEightyFour, BookId.From(9));

    var sapiens = new Book(
      westLibrary.Id,
      history.Id,
      BookTitle.From("Sapiens"),
      AuthorName.From("Yuval Noah Harari"),
      Isbn.From("978-0062316097"),
      new InventoryState(4, 4),
      BookDescription.From("A sweeping narrative of how Homo sapiens shaped the modern world."),
      CoverImageReference.From("/covers/sapiens.jpg"));
    SetId(sapiens, BookId.From(10));

    var gunsGermsAndSteel = new Book(
      westLibrary.Id,
      history.Id,
      BookTitle.From("Guns, Germs, and Steel"),
      AuthorName.From("Jared Diamond"),
      Isbn.From("978-0393354324"),
      new InventoryState(4, 4),
      BookDescription.From("A broad history of how geography and environment influenced human societies."),
      CoverImageReference.From("/covers/guns-germs-and-steel.jpg"));
    SetId(gunsGermsAndSteel, BookId.From(11));

    var briefHistoryOfTime = new Book(
      riversideLibrary.Id,
      science.Id,
      BookTitle.From("A Brief History of Time"),
      AuthorName.From("Stephen Hawking"),
      Isbn.From("978-0553380163"),
      new InventoryState(5, 5),
      BookDescription.From("An accessible introduction to cosmology, black holes, and the origins of the universe."),
      CoverImageReference.From("/covers/brief-history-of-time.jpg"));
    SetId(briefHistoryOfTime, BookId.From(12));

    var psychologyOfMoney = new Book(
      centralLibrary.Id,
      business.Id,
      BookTitle.From("The Psychology of Money"),
      AuthorName.From("Morgan Housel"),
      Isbn.From("978-9390166268"),
      new InventoryState(5, 5),
      BookDescription.From("A readable exploration of how behavior shapes financial decisions and long-term outcomes."),
      CoverImageReference.From("/covers/psychology-of-money.jpg"));
    SetId(psychologyOfMoney, BookId.From(13));

    dbContext.Books.AddRange(
      cleanCode,
      pragmaticProgrammer,
      refactoring,
      designingDataIntensiveApplications,
      atomicHabits,
      deepWork,
      hobbit,
      dune,
      nineteenEightyFour,
      sapiens,
      gunsGermsAndSteel,
      briefHistoryOfTime,
      psychologyOfMoney);
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

    var returnedRental = Rental.Create(
      reader.Id,
      sapiens.Id,
      westLibrary.Id,
      DeliveryAddressSnapshot.From(reader.SavedAddresses.Single(x => x.IsDefault).Address),
      DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-24)));
    SetId(returnedRental, RentalId.From(3));
    returnedRental.MarkDelivered();
    sapiens.ReserveCopy();
    returnedRental.RequestReturn(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-10)));
    returnedRental.CompleteReturn(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-8)));
    sapiens.ReturnCopy();

    var pendingDeliveryRental = Rental.Create(
      reader.Id,
      atomicHabits.Id,
      westLibrary.Id,
      DeliveryAddressSnapshot.From(reader.SavedAddresses.Single(x => x.IsDefault).Address),
      DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)));
    SetId(pendingDeliveryRental, RentalId.From(4));
    atomicHabits.ReserveCopy();

    var overdueRental = Rental.Create(
      secondReader.Id,
      deepWork.Id,
      westLibrary.Id,
      DeliveryAddressSnapshot.From(secondReader.SavedAddresses.Single(x => x.IsDefault).Address),
      DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-20)));
    SetId(overdueRental, RentalId.From(5));
    overdueRental.MarkDelivered();
    overdueRental.MarkOverdueIfNeeded(DateOnly.FromDateTime(DateTime.UtcNow.Date));
    deepWork.ReserveCopy();

    var returnedRentalTwo = Rental.Create(
      secondReader.Id,
      nineteenEightyFour.Id,
      westLibrary.Id,
      DeliveryAddressSnapshot.From(secondReader.SavedAddresses.Single(x => x.IsDefault).Address),
      DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30)));
    SetId(returnedRentalTwo, RentalId.From(6));
    returnedRentalTwo.MarkDelivered();
    nineteenEightyFour.ReserveCopy();
    returnedRentalTwo.RequestReturn(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-18)));
    returnedRentalTwo.CompleteReturn(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-16)));
    nineteenEightyFour.ReturnCopy();

    var failedDeliveryRental = Rental.Create(
      secondReader.Id,
      dune.Id,
      centralLibrary.Id,
      DeliveryAddressSnapshot.From(secondReader.SavedAddresses.Single(x => x.IsDefault).Address),
      DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-2)));
    SetId(failedDeliveryRental, RentalId.From(7));
    dune.ReserveCopy();

    dbContext.Rentals.AddRange(
      activeRental,
      pickupRental,
      returnedRental,
      pendingDeliveryRental,
      overdueRental,
      returnedRentalTwo,
      failedDeliveryRental);
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

    var scheduledDeliveryTask = new DeliveryTask(
      pendingDeliveryRental.Id,
      secondDeliveryPartner.Id,
      DeliveryTaskType.Delivery,
      new DeliveryAddressSnapshot(
        pendingDeliveryRental.DeliveryAddress.Street,
        pendingDeliveryRental.DeliveryAddress.City,
        pendingDeliveryRental.DeliveryAddress.State,
        pendingDeliveryRental.DeliveryAddress.PostalCode,
        pendingDeliveryRental.DeliveryAddress.Country),
      DateTimeOffset.UtcNow.AddHours(-2));
    SetId(scheduledDeliveryTask, DeliveryTaskId.From(3));

    var completedDeliveryTask = new DeliveryTask(
      overdueRental.Id,
      secondDeliveryPartner.Id,
      DeliveryTaskType.Delivery,
      new DeliveryAddressSnapshot(
        overdueRental.DeliveryAddress.Street,
        overdueRental.DeliveryAddress.City,
        overdueRental.DeliveryAddress.State,
        overdueRental.DeliveryAddress.PostalCode,
        overdueRental.DeliveryAddress.Country),
      DateTimeOffset.UtcNow.AddDays(-19));
    SetId(completedDeliveryTask, DeliveryTaskId.From(4));
    completedDeliveryTask.MarkInTransit();
    completedDeliveryTask.Complete(DateTimeOffset.UtcNow.AddDays(-18));

    var failedDeliveryTask = new DeliveryTask(
      failedDeliveryRental.Id,
      deliveryPartner.Id,
      DeliveryTaskType.Delivery,
      new DeliveryAddressSnapshot(
        failedDeliveryRental.DeliveryAddress.Street,
        failedDeliveryRental.DeliveryAddress.City,
        failedDeliveryRental.DeliveryAddress.State,
        failedDeliveryRental.DeliveryAddress.PostalCode,
        failedDeliveryRental.DeliveryAddress.Country),
      DateTimeOffset.UtcNow.AddDays(-1));
    SetId(failedDeliveryTask, DeliveryTaskId.From(5));
    failedDeliveryTask.MarkInTransit();
    failedDeliveryTask.Fail();

    dbContext.DeliveryTasks.AddRange(deliveryTask, pickupTask, scheduledDeliveryTask, completedDeliveryTask, failedDeliveryTask);

    var review = new BookReview(
      sapiens.Id,
      reader.Id,
      StarRating.From(5),
      ReviewTitle.From("Compelling and thoughtful"),
      ReviewContent.From("A crisp overview of human history with ideas that stay with you after the last chapter."),
      DateTimeOffset.UtcNow.AddDays(-6));
    SetId(review, BookReviewId.From(1));
    sapiens.ApplyRating(review.Rating.Value);

    var reviewTwo = new BookReview(
      nineteenEightyFour.Id,
      secondReader.Id,
      StarRating.From(5),
      ReviewTitle.From("Still unsettling"),
      ReviewContent.From("A sharp and memorable dystopian novel that feels relevant long after it ends."),
      DateTimeOffset.UtcNow.AddDays(-12));
    SetId(reviewTwo, BookReviewId.From(2));
    nineteenEightyFour.ApplyRating(reviewTwo.Rating.Value);

    dbContext.BookReviews.AddRange(review, reviewTwo);
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
