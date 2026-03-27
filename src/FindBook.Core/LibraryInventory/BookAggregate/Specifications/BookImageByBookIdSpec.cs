namespace FindBook.Core.LibraryInventory.BookAggregate.Specifications;

public sealed class BookImageByBookIdSpec : Specification<BookImage>, ISingleResultSpecification<BookImage>
{
  public BookImageByBookIdSpec(BookId bookId)
  {
    Query.Where(x => x.BookId == bookId);
  }
}
