using Vogen;

namespace FindBook.Core.SharedKernel;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct PhoneNumber
{
  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Phone number is required.")
      : value.Length > 32
        ? Validation.Invalid("Phone number is too long.")
        : Validation.Ok;
}
