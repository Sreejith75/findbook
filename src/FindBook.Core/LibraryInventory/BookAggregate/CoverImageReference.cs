using Vogen;

namespace FindBook.Core.LibraryInventory.BookAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct CoverImageReference
{
  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Cover image reference is required.")
      : Validation.Ok;
}
