using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;

namespace FindBook.UnitTests.Core.LibraryInventory;

public class BookBehavior
{
  [Fact]
  public void ReserveCopyReducesAvailability()
  {
    var book = CreateBook();

    book.ReserveCopy();

    book.InventoryState.AvailableCopies.ShouldBe(2);
    book.IsAvailable.ShouldBeTrue();
  }

  [Fact]
  public void ReturnCopyRestoresAvailability()
  {
    var book = CreateBook();
    book.ReserveCopy();

    book.ReturnCopy();

    book.InventoryState.AvailableCopies.ShouldBe(3);
  }

  [Fact]
  public void ApplyRatingUpdatesSummary()
  {
    var book = CreateBook();

    book.ApplyRating(5);
    book.ApplyRating(3);

    book.RatingSummary.RatingCount.ShouldBe(2);
    book.RatingSummary.AverageRating.ShouldBe(4.00m);
  }

  private static Book CreateBook() =>
    new(
      LibraryId.From(1),
      CategoryId.From(2),
      BookTitle.From("Domain-Driven Design"),
      AuthorName.From("Eric Evans"),
      Isbn.From("978-0321125217"),
      new InventoryState(3, 3),
      BookDescription.From("A domain design book."));
}
