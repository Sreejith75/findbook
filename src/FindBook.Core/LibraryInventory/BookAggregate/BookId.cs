using Vogen;

namespace FindBook.Core.LibraryInventory.BookAggregate;

[ValueObject<int>]
public readonly partial struct BookId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Book ID must be positive.");
}
