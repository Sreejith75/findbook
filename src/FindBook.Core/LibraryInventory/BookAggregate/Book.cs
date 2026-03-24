using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;

namespace FindBook.Core.LibraryInventory.BookAggregate;

public class Book : EntityBase<Book, BookId>, IAggregateRoot
{
  private Book() { }

  public Book(
    LibraryId libraryId,
    CategoryId categoryId,
    BookTitle title,
    AuthorName author,
    Isbn isbn,
    InventoryState inventoryState,
    BookDescription? description = null,
    CoverImageReference? coverImageReference = null)
  {
    LibraryId = libraryId;
    CategoryId = categoryId;
    Title = title;
    Author = author;
    Isbn = isbn;
    InventoryState = inventoryState;
    Description = description;
    CoverImageReference = coverImageReference;
    RatingSummary = BookRatingSummary.Empty;
  }

  public LibraryId LibraryId { get; private set; }
  public CategoryId CategoryId { get; private set; }
  public BookTitle Title { get; private set; }
  public AuthorName Author { get; private set; }
  public Isbn Isbn { get; private set; }
  public BookDescription? Description { get; private set; }
  public InventoryState InventoryState { get; private set; } = null!;
  public BookRatingSummary RatingSummary { get; private set; } = BookRatingSummary.Empty;
  public CoverImageReference? CoverImageReference { get; private set; }
  public bool IsAvailable => InventoryState.HasAvailableCopies;

  public void UpdateDetails(
    CategoryId categoryId,
    BookTitle title,
    AuthorName author,
    Isbn isbn,
    InventoryState inventoryState,
    BookDescription? description,
    CoverImageReference? coverImageReference)
  {
    CategoryId = categoryId;
    Title = title;
    Author = author;
    Isbn = isbn;
    InventoryState = inventoryState;
    Description = description;
    CoverImageReference = coverImageReference;
  }

  public void ReserveCopy() => InventoryState = InventoryState.ReserveCopy();

  public void ReturnCopy() => InventoryState = InventoryState.ReturnCopy();

  public void ApplyRating(int rating) => RatingSummary = RatingSummary.AddRating(rating);
}
