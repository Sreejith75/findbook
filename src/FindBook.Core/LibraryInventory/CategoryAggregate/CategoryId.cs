using Vogen;

namespace FindBook.Core.LibraryInventory.CategoryAggregate;

[ValueObject<int>]
public readonly partial struct CategoryId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Category ID must be positive.");
}
