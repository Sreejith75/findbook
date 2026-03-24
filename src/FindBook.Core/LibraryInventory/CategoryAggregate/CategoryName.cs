using Vogen;

namespace FindBook.Core.LibraryInventory.CategoryAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct CategoryName
{
  public const int MaxLength = 120;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Category name is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Category name cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
