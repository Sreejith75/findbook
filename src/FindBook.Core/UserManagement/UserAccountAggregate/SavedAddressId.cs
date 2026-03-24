using Vogen;

namespace FindBook.Core.UserManagement.UserAccountAggregate;

[ValueObject<int>]
public readonly partial struct SavedAddressId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Saved address ID must be positive.");
}
