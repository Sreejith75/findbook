using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;

namespace FindBook.Core.LibraryInventory.BookAggregate.Specifications;

public sealed class ListBooksSpec : Specification<Book>
{
  public ListBooksSpec(string? q = null, LibraryId? libraryId = null, CategoryId? categoryId = null, bool availableOnly = false)
  {
    if (!string.IsNullOrWhiteSpace(q))
    {
      var term = q.Trim();
      Query.Where(x =>
        x.Title.Value.Contains(term) ||
        x.Author.Value.Contains(term) ||
        x.Isbn.Value.Contains(term));
    }

    if (libraryId.HasValue)
    {
      Query.Where(x => x.LibraryId == libraryId.Value);
    }

    if (categoryId.HasValue)
    {
      Query.Where(x => x.CategoryId == categoryId.Value);
    }

    if (availableOnly)
    {
      Query.Where(x => x.InventoryState.AvailableCopies > 0);
    }

    Query.OrderBy(x => x.Title);
  }
}
