using Vogen;

namespace FindBook.Core.LibraryInventory.LibraryAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct LibraryName
{
  public const int MaxLength = 200;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Library name is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Library name cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
