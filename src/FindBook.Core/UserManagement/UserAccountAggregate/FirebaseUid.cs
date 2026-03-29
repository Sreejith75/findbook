using Vogen;

namespace FindBook.Core.UserManagement.UserAccountAggregate;

[ValueObject<string>]
public readonly partial struct FirebaseUid
{
  private static Validation Validate(string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Firebase UID is required.")
      : Validation.Ok;
}
