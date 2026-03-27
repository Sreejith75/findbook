namespace FindBook.Core.LibraryInventory.CategoryAggregate.Specifications;

public sealed class CategoryByNameSpec : Specification<Category>, ISingleResultSpecification<Category>
{
  public CategoryByNameSpec(CategoryName name)
  {
    Query.Where(x => x.Name == name);
  }
}
