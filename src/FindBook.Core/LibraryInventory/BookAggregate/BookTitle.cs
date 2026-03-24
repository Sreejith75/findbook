using Vogen;

namespace FindBook.Core.LibraryInventory.BookAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct BookTitle
{
  public const int MaxLength = 300;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Book title is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Book title cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
