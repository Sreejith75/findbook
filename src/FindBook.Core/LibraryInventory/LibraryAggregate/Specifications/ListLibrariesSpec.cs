namespace FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;

public sealed class ListLibrariesSpec : Specification<Library>
{
  public ListLibrariesSpec()
  {
    Query.OrderBy(x => x.Name);
  }
}
