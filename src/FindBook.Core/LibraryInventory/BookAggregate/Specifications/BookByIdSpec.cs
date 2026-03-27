namespace FindBook.Core.LibraryInventory.BookAggregate.Specifications;

public sealed class BookByIdSpec : Specification<Book>, ISingleResultSpecification<Book>
{
  public BookByIdSpec(BookId id)
  {
    Query.Where(x => x.Id == id);
  }
}
