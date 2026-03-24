using Vogen;

namespace FindBook.Core.SharedKernel;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct PersonName
{
  public const int MaxLength = 200;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Person name is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Person name cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
