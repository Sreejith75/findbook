namespace FindBook.Core.LibraryInventory.CategoryAggregate;

public class Category : EntityBase<Category, CategoryId>, IAggregateRoot
{
  private Category() { }

  public Category(CategoryName name)
  {
    Name = name;
  }

  public CategoryName Name { get; private set; }

  public void Rename(CategoryName name) => Name = name;
}
