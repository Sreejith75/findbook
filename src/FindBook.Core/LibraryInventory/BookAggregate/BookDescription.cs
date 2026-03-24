using Vogen;

namespace FindBook.Core.LibraryInventory.BookAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct BookDescription
{
  public const int MaxLength = 4000;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Book description is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Book description cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
