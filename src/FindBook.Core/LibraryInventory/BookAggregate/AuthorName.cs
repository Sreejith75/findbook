using Vogen;

namespace FindBook.Core.LibraryInventory.BookAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct AuthorName
{
  public const int MaxLength = 200;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Author name is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Author name cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
