using Vogen;

namespace FindBook.Core.UserManagement.UserAccountAggregate;

[ValueObject<int>]
public readonly partial struct UserAccountId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("User account ID must be positive.");
}
