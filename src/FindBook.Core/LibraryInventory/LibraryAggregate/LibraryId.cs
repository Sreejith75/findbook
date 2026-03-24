using Vogen;

namespace FindBook.Core.LibraryInventory.LibraryAggregate;

[ValueObject<int>]
public readonly partial struct LibraryId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Library ID must be positive.");
}
