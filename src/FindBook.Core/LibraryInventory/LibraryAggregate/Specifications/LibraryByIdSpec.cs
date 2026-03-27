namespace FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;

public sealed class LibraryByIdSpec : Specification<Library>, ISingleResultSpecification<Library>
{
  public LibraryByIdSpec(LibraryId id)
  {
    Query.Where(x => x.Id == id);
  }
}
