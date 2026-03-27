namespace FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;

public sealed class LibraryByNameSpec : Specification<Library>, ISingleResultSpecification<Library>
{
  public LibraryByNameSpec(LibraryName name)
  {
    Query.Where(x => x.Name == name);
  }
}
