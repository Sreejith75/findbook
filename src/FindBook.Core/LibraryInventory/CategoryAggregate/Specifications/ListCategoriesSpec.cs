namespace FindBook.Core.LibraryInventory.CategoryAggregate.Specifications;

public sealed class ListCategoriesSpec : Specification<Category>
{
  public ListCategoriesSpec()
  {
    Query.OrderBy(x => x.Name);
  }
}
