using Vogen;

namespace FindBook.Core.LibraryInventory.BookAggregate;

[ValueObject<int>]
public readonly partial struct BookImageId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Book image ID must be positive.");
}
