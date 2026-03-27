namespace FindBook.Core.LibraryInventory.CategoryAggregate.Specifications;

public sealed class CategoryByIdSpec : Specification<Category>, ISingleResultSpecification<Category>
{
  public CategoryByIdSpec(CategoryId id)
  {
    Query.Where(x => x.Id == id);
  }
}
