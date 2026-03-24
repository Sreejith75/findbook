using Vogen;

namespace FindBook.Core.SharedKernel;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct PasswordHash
{
  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Password hash is required.")
      : value.Length < 8
        ? Validation.Invalid("Password hash is too short.")
        : Validation.Ok;
}
