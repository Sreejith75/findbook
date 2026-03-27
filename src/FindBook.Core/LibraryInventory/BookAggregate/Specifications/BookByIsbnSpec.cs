namespace FindBook.Core.LibraryInventory.BookAggregate.Specifications;

public sealed class BookByIsbnSpec : Specification<Book>, ISingleResultSpecification<Book>
{
  public BookByIsbnSpec(Isbn isbn)
  {
    Query.Where(x => x.Isbn == isbn);
  }
}
